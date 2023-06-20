using Microsoft.VisualBasic;
using MQTTnet.Client;
//using Newtonsoft.Json;
using System.CodeDom;
using System.Diagnostics;
using System.Text.Json;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public event EventHandler<string>? StatusMessage;

        public delegate void OnPublished(InformationAdapter information);

        public string? Name { get; private set; }

        public bool IsConnected => _mqtt.IsConnected;

        public ProcessCatalog Processes { get; private set; }
        internal ContextAdapter Context { get; private set; }

        Dictionary<string, OnPublished> _publishCallbacks = new Dictionary<string, OnPublished>();

        private MqttClient _mqtt;

        public Identity Identity { get; }

        public TechnologaiAgent(string authUri, string clientId, string clientSecret, string memberId)
        {
            Identity = new Identity(authUri, clientId, clientSecret, memberId);
            Processes = new ProcessCatalog(Identity, this);
            Context = new ContextAdapter(Identity);

            _mqtt = new MqttClient(Identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        // ** TRANSPORT **

        private async void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var brokerMessage = BrokerMessage.FromMqttArgs(args);

            if (brokerMessage.AgentMessage?.Type == AgentMessageType.INFORMATION)
            {
                Information? information = brokerMessage.AgentMessage?.Data as Information;

                if (information != null)
                {
                    await Receive(InformationAdapter.Create(this, information));
                }
            }

            else if (brokerMessage.AgentMessage?.Type == AgentMessageType.PROCESS)
            {
                IProcess? process = brokerMessage.AgentMessage?.Data as IProcess;

                if (process != null && process.WorkerId != Identity.Id)
                {
                    Processes.Add(process, false);
                    await SendStatusMessage($"Received: {process.Id}");
                }
            }
        }

        private async Task Receive(InformationAdapter information)
        {
            Context.Add(information);

            // Closed and this agent is the creator
            if (information.State == InformationState.CLOSED && information.CreatorId == Identity.Id)
            {
                //_active.Remove(information.ContextId);

                // Activate the calling information
                var parentInformation = Context.GetCreator(information.Id);

                if (parentInformation == null)
                {
                    // This is a root request. End here and send a callback.                        
                    _publishCallbacks[information.Id]?.Invoke(information);
                    return;
                }
                information = InformationAdapter.Create(this, parentInformation);
            }

            // Closed, and this agent is not the creator
            if (information.State == InformationState.CLOSED && information.CreatorId != Identity.Id)
            {
                // TODO: Review
            }

            // Open, and this agent is the worker
            if (information.State == InformationState.OPEN && information.WorkerId == Identity.Id)
            {
                //_active[information.ContextId] = information;

                switch (information.ProcessState)
                {
                    // TODO: Debounce

                    case ProcessState.ASSESS:
                        await information.Assess();
                        break;
                    case ProcessState.EXECUTE:
                        await information.Execute();
                        break;
                    case ProcessState.SPAWN:
                        await information.Spawn();
                        break;
                }
            }
        }

        public async Task<InformationAdapter> Create(string processId, string? input = null)
        {
            //_active[information.ContextId] = information;  
            return await InformationAdapter.Create(this, Processes[processId], input);
        }

        public async void PublishWithCallback(InformationAdapter information, OnPublished onPublished)
        {
            _publishCallbacks.Add(information.Id, onPublished);
            await Publish(information);
        }

        public async Task Publish(InformationAdapter information)
        {
            // SendStatusMessage($"{information.ContextId} Publish> {information.ProcessId} | {information.Input} | {information.Output}");

            // TODO: short circuit.
            /*
            if (Identity.Id == information.OwnerId)
            {
                Receive(information);
                return Task.CompletedTask;
            }*/

            if (information.State == InformationState.DRAFT)
            {
                information.State = InformationState.OPEN;
            }

            AgentMessage agentMessage = new AgentMessage()
            {
                Data = information,
                Type = AgentMessageType.INFORMATION
            };

            var brokerMessage = new BrokerMessage(Identity)
            {
                AgentMessage = agentMessage,
                MemberId = information.WorkerId
            };

            await _mqtt.PublishAsync(brokerMessage.Topic, brokerMessage.ConvertAgentMessageToString());
        }

        public async Task BroadcastProcesses()
        {
            // await SendStatusMessage($"Broadcasting Processes");

            foreach (var process in Processes.Values)
            {
                if (process.WorkerId == Identity.Id || process.WorkerId == null)
                {
                    await Broadcast(process);
                }
            }
        }

        public async Task Broadcast(IProcess process)
        {
            await SendStatusMessage($"Broadcasting: {process.Id}");

            process.WorkerId = Identity.Id;

            AgentMessage agentMessage = new AgentMessage()
            {
                Data = process,
                Type = AgentMessageType.PROCESS
            };

            var brokerMessage = new BrokerMessage(Identity)
            {
                AgentMessage = agentMessage,
                MemberId = "0"
            };

            string agentMessageJson = brokerMessage.ConvertAgentMessageToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, agentMessageJson);
        }

        internal async Task SendStatusMessage(string message)
        {
            if (_mqtt.IsConnected && Processes.ContainsKey("display_log_message"))
            {
                var information = await Create("display_log_message", message);
                await information.Publish();
            }
            else
            {
                StatusMessage?.Invoke(this, message);
            }
        }

        // Startup

        public async Task Start()
        {
            // TODO: Fix in AI-17
            await SendStatusMessage($"Warming up...");
            await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/jwks.json");
            await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/openid-configuration");

            await Identity.Authenticate(Identity.Authority.BrokerUri);

            this.Name = Identity.Name;

            await SendStatusMessage($"Authenticated");

            await _mqtt.ConnectAsync();
            await SendStatusMessage($"Connected");

            await _mqtt.SubscribeAsync(Identity.SubscribeAgencyMask);
            await SendStatusMessage($"Agency Subscribed> {Identity.SubscribeAgencyMask}");

            await _mqtt.SubscribeAsync(Identity.SubscribeMemberMask);
            await SendStatusMessage($"Member Subscribed> {Identity.SubscribeMemberMask}");

            await BroadcastProcesses();
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}