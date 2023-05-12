using MQTTnet.Client;
using Newtonsoft.Json;

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

            if (brokerMessage.IsBroadcast)
            {
                // HERE

                // TODO: Handle broadcast
                string foo = "bar";
                // : AnnounceProcess
                // : Context related
            }

            else if (brokerMessage.Information != null)
            {
                Receive(new InformationAdapter(this, brokerMessage.Information)).Wait();
            }

        }

        private async Task Receive(InformationAdapter information)
        {
            Context.Add(information);

            if (information.State == InformationState.CLOSED && information.CreatorId == Identity.Id)
            {
                //_active.Remove(information.ContextId);

                // Activate the calling information
                var creator = Context.GetCreator(information.ContextId);

                if (creator == null)
                {
                    // This is a root request. End here and send a callback.                        
                    _publishCallbacks[information.ContextId]?.Invoke(information);
                    return;
                }
                information = new InformationAdapter(this, creator);
            }

            if (information.State == InformationState.CLOSED && information.CreatorId != Identity.Id)
            {
                // This is closed but I'm not the creator. Sent to me for review.
            }

            if (information.State == InformationState.OPEN && information.WorkerId == Identity.Id)
            {
                //_active[information.ContextId] = information;

                var assessment = await information.Assess();

                if (assessment.Result == AssessmentResult.EXECUTE)
                {
                    // TODO: Debounce?
                    await information.Execute(assessment);
                }
                else if (assessment.Result == AssessmentResult.SPAWN)
                {
                    await information.Spawn(assessment);
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
            _publishCallbacks.Add(information.ContextId, onPublished);
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
                information.Information.State = InformationState.OPEN;
            }

            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = information.WorkerId
            };

            await _mqtt.PublishAsync(message.Topic, message.Information.ToJson());
        }

        public async Task Broadcast(IProcess process)
        {
            await SendStatusMessage($"{process.Id} Broadcast");

            process.MemberId = Identity.Id;

            var message = new BrokerMessage(Identity)
            {
                Process = process,
                MemberId = "0"
            };

            await _mqtt.PublishAsync(message.Topic, JsonConvert.SerializeObject(message.Process));
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