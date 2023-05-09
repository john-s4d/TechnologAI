using MQTTnet.Client;

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
            Processes = new ProcessCatalog(Identity);
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
                // TODO: Handle broadcast
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

        public InformationAdapter Create(string abilityName, string? input = null)
        {
            var information = InformationAdapter.Create(this, abilityName, input);
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
            SendStatusMessage($"{information.ContextId} Publish> {information.ProcessId} | {information.Input} | {information.Output}");

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

            var topic = message.Topic;

            await _mqtt.PublishAsync(topic, message.Information.ToJson());
        }

        public async Task Broadcast(InformationAdapter information)
        {
            information.Information.State = InformationState.CLOSED;

            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = "0"
            };

            var topic = message.Topic;

            await _mqtt.PublishAsync(topic, message.Information.ToJson());
        }

        internal void SendStatusMessage(string message)
        {
            //await Create("display_log_message", message).Publish();

            StatusMessage?.Invoke(this, message);
        }

        // Startup

        public async Task Start()
        {
            try
            {
                // TODO: Fix in AI-17
                SendStatusMessage($"Warming up...");
                await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/jwks.json");
                await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/openid-configuration");

                await Identity.Authenticate(Identity.Authority.BrokerUri);

                this.Name = Identity.Name;

                SendStatusMessage($"Authenticated");

                await _mqtt.ConnectAsync();
                SendStatusMessage($"Connected");

                //await _mqtt.SubscribeAsync(Identity.SubscribeAgencyMask);
                //SendStatusMessage($"Agency Subscribed> {Identity.SubscribeAgencyMask}");

                await _mqtt.SubscribeAsync(Identity.SubscribeMemberMask);
                SendStatusMessage($"Member Subscribed> {Identity.SubscribeMemberMask}");

            }
            catch (Exception ex)
            {
                SendStatusMessage(ex.ToString());
            }
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}