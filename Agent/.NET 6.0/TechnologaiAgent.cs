using Microsoft.IdentityModel.Tokens;
using MQTTnet.Client;

namespace Technologai
{
    public abstract class TechnologaiAgent
    {
        public event EventHandler<string>? StatusMessage;

        public string? Name { get; private set; }
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
                Receive(new InformationAdapter(information, this)).Wait();
            }
        }

        private async Task Receive(InformationAdapter information)
        {
            if (await OnReceive(information))
            {
                await Publish(information);
            }
        }

        // This is information received on the member channel, addressed to me.
        private async Task<bool> OnReceive(InformationAdapter information)
        {
            // Kill Drafts
            if (information.State == InformationState.DRAFT)
            {
                return false;
            }

            // Forward information that doesn't belong to me. Shouldn't receive these. 
            if (information.OwnerId != Identity.Id)
            {
                await Publish(information);
                return false;
            }

            // Creator

            if (information.CreatorId == Identity.Id && information.State == InformationState.OPEN)
            {
                await information.Execute();
                await information.Assess();
                return false;
            }

            if (information.CreatorId == Identity.Id && information.State == InformationState.CLOSED)
            {
                await information.Assess();
                return false;
            }

            // Owner Open (Not Creator)

            if (information.State == InformationState.OPEN)
            {
                // Work on it
                await information.Execute();
                return true;
            }

            // Owner Closed (Not Creator)

            if (information.State == InformationState.CLOSED)
            {
                // Someone sent me something inetresting.
                await information.Review();
                return true;
            }

            throw new Exception("Unhandled Information");
        }

        protected internal abstract Task Execute(Ability ability, InformationAdapter information);
        protected internal abstract Task Assess(InformationAdapter information);
        protected internal abstract Task Review(InformationAdapter information);

        public async Task Publish(InformationAdapter information)
        {
            // Open drafts
            if (information.State == InformationState.DRAFT)
            {
                information.State = InformationState.OPEN;
            }

            if (Identity.Id == information.OwnerId)
            {
                await Receive(information);
                return;
            }

            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = information.OwnerId
            };

            string? topic = Identity.GetMaskedTopic(message.TopicMember);

            await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

            SendStatusMessage($"{message.Information.ContextId} Publish>  {message.Information.AbilityName} | {message.Information.Input} | {message.Information.Output}");
        }

        public InformationAdapter Create(string ability, string? input = null)
        {
            // TODO: We might need to find the ability first.
            return InformationAdapter.Create(this, Abilities[ability], input);
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