using MQTTnet.Client;

namespace Technologai
{
    public abstract class TechnologaiAgent
    {
        public string? Name { get; private set; }
        public event EventHandler<string>? StatusMessage;

        public AbilityCatalog Abilities { get; private set; }
        internal ContextProvider Context { get; private set; }

        private MqttClient _mqtt;

        private Dictionary<string, Information> _localInformation = new Dictionary<string, Information>();        

        public Identity Identity { get; }

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Identity = new Identity(authorityName, clientId, clientSecret, memberId);
            Abilities = new AbilityCatalog(Identity);
            Context = new ContextProvider(Identity);

            _mqtt = new MqttClient(Identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var information = BrokerMessage.FromMqttArgs(args).Information;

            if (information != null)
            {
                Receive(new InformationHandler(information, this)).Wait();
            }
        }

        private async Task Receive(InformationHandler information)
        {
            if (await BeforeHandle(information))
            {
                if (!string.IsNullOrEmpty(information.AbilityName) &&
                    Abilities.ContainsKey(information.AbilityName))
                {
                    await Execute(Abilities[information.AbilityName], information);
                }
                await Handle(information);
                await Publish(information);
            }
        }

        private async Task<bool> BeforeHandle(InformationHandler information)
        {
            var isNullOwned = information.OwnerId == null;
            var isSelfOwned = information.OwnerId == Identity.Id;
            var isElseOwned = information.OwnerId != Identity.Id && information.OwnerId != null;
            var isDraft = information.State == InformationState.DRAFT;
            var isOpen = information.State == InformationState.OPEN;
            var isClosed = information.State == InformationState.CLOSED;
            var isCreator = information.CreatorId == Identity.Id;

            if (isDraft)
            {
                return false;
            }

            if (isElseOwned)
            {
                await SendToBroker(information, information.OwnerId);
                return false;
            }

            if (!_localInformation.ContainsKey(information.ContextId))
            {
                _localInformation.Add(information.ContextId, information);
            }


            if (isCreator && isOpen)
            {
                information.Close(); // Incomplete
                information.Defer();
                return false;
            }

            if (isCreator && isClosed)
            {
                information.Compile();
                return false;
            }

            if (isClosed)
            {
                await SendToBroker(information, information.CreatorId);
                return false;
            }

            return true;
        }

        public abstract Task Handle(InformationHandler information);

        public async Task Publish(InformationHandler information)
        {
            var isNullOwned = information.OwnerId == null;
            var isSelfOwned = information.OwnerId == Identity.Id;
            var isElseOwned = information.OwnerId != Identity.Id && information.OwnerId != null;
            var isDraft = information.State == InformationState.DRAFT;
            var isOpen = information.State == InformationState.OPEN;
            var isClosed = information.State == InformationState.CLOSED;
            var isCreator = information.CreatorId == Identity.Id;

            if (isDraft)
            {
                information.State = InformationState.OPEN;
            }

            if (!_localInformation.ContainsKey(information.ContextId))
            {
                _localInformation.Add(information.ContextId, information);
            }

            if (isElseOwned)
            {
                await SendToBroker(information, information.OwnerId);
                return;
            }

            if (isSelfOwned && isOpen)
            {
                information.Defer();
                return;
            }

            await SendToBroker(information, information.CreatorId);            
        }

        public abstract Task Execute(Ability ability, InformationHandler information);

        public InformationHandler CreateInformation(string? input = null)
        {
            return InformationHandler.CreateInformation(this, input);
        }

        // Message Handling

        private async Task<bool> SendToBroker(Information information, string memberId)
        {
            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = memberId
            };

            string? topic = Identity.GetMaskedTopic(message.Topic);

            await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

            SendStatusMessage($"Publish> {message.Information.ContextId} | {message.Information.Input} | {message.Information.Output}");

            return true;
        }

        internal void SendStatusMessage(string message)
        {
            StatusMessage?.Invoke(this, message);
        }

        // Startup

        public async Task Start()
        {
            try
            {
                // TODO: Fix in AI-17
                SendStatusMessage($"Warming up...");
                await new HttpClient().GetAsync($"https://{Identity.Authority.Host}/.well-known/jwks.json");
                await new HttpClient().GetAsync($"https://{Identity.Authority.Host}/.well-known/openid-configuration");

                await Identity.Authenticate();

                this.Name = Identity.Name;

                SendStatusMessage($"Authenticated");

                await _mqtt.ConnectAsync();
                SendStatusMessage($"Connected");

                await _mqtt.SubscribeAsync(Identity.SubscribeAgencyMask);
                SendStatusMessage($"Agency Subscribed> {Identity.SubscribeAgencyMask}");

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