using MQTTnet.Client;

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

            if (brokerMessage.MessageType == AgentMessageType.INFORMATION)
            {
                Information? information = brokerMessage.MessageData as Information;

                if (information != null)
                {
                    await Receive(InformationAdapter.Create(this, information));
                }
            }

            else if (brokerMessage.MessageType == AgentMessageType.PROCESS)
            {
                IProcess? process = brokerMessage.MessageData as IProcess;

                if (process != null && process.MemberId != Identity.Id)
                {
                    Processes.Add(process);
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
            else if (information.State == InformationState.CLOSED && information.CreatorId != Identity.Id)
            {
                // TODO: Review. Add to Context.
            }

            // Open, and this agent is assigned
            else if (information.State == InformationState.OPEN && information.MemberId == Identity.Id)
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
            SendStatusMessage($"{information.Id} Publish> {information.ProcessId} | {information.InputText} | {information.OutputText}");

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

            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.INFORMATION,
                MessageData = information,
                MemberId = information.MemberId
            };

            await _mqtt.PublishAsync(brokerMessage.Topic, brokerMessage.ConvertMessageDataToString(), brokerMessage.MessageType);
        }

        public async Task BroadcastProcesses()
        {
            // await SendStatusMessage($"Broadcasting Processes");

            foreach (var process in Processes.Values)
            {
                if (process.MemberId == Identity.Id || process.MemberId == null)
                {
                    await Broadcast(process);
                }
            }
        }

        public async Task Broadcast(Process process)
        {
            await SendStatusMessage($"Broadcasting: {process.Id}");

            process.MemberId = Identity.Id;

            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.PROCESS,
                MessageData = process,                
                MemberId = "0"
            };

            string messageJson = brokerMessage.ConvertMessageDataToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, messageJson, brokerMessage.MessageType);
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