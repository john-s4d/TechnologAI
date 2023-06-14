using Microsoft.VisualBasic;
using MQTTnet.Client;
//using Newtonsoft.Json;
using System.CodeDom;
using System.Diagnostics;
using System.Text.Json;

namespace Technologai
{
    public abstract class TechnologaiAgent
    {
        public event EventHandler<string>? StatusMessage;

        public delegate void OnPublished(InformationAdapter information);

        public string? Name { get; private set; }

        public ProcessCatalog Processes { get; private set; }
        internal ContextAdapter Context { get; private set; }
        //private Dictionary<string, Information> _active { get; } = new();

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

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var brokerMessage = BrokerMessage.FromMqttArgs(args);

            if (brokerMessage.AgentMessage?.Type == AgentMessageType.INFORMATION)
            {
                Information? information = brokerMessage.AgentMessage?.Data as Information;

                if (information != null)
                {
                    Receive(InformationAdapter.Create(this, information)).Wait();
                }
            }

            else if (brokerMessage.AgentMessage?.Type == AgentMessageType.PROCESS)
            {
                IProcess? process = brokerMessage.AgentMessage?.Data as IProcess;

                if (process != null)
                {
                    Processes.Add(process, false);
                    SendStatusMessage($"Received: {process.Id}").Wait();
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
                var creator = Context.GetCreator(information.Id);

                if (creator == null)
                {
                    // This is a root request. End here and send a callback.                        
                    _publishCallbacks[information.Id]?.Invoke(information);
                    return;
                }
                information = InformationAdapter.Create(this, creator);
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

                switch (information.Process.State)
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

        public InformationAdapter Create(string processId, string? input = null)
        {
            var information = InformationAdapter.Create(this, processId, input);
            //_active[information.ContextId] = information;
            return information;
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
                await Create("display_log_message", message).Publish();
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
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}