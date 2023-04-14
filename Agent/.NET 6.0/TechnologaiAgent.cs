using Microsoft.VisualBasic;
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

        public async Task Receive(InformationHandler information)
        {
            if (await BeforeHandle(information))
            {   
                await Publish(information);
            }
        }

        // This is information received on the member channel, addressed to me.
        private async Task<bool> BeforeHandle(InformationHandler information)
        {
            // Reject Drafts
            if (information.State == InformationState.DRAFT)
            {
                return false;
            }

            // Forward information that doesn't belong to me. Shouldn't receive these. 
            if (information.OwnerId != Identity.Id)
            {
                await SendToBroker(information, information.OwnerId);
                return false;
            }

            // I'm the owner

            // Incomplete
            if (information.CreatorId == Identity.Id && information.State == InformationState.OPEN)
            {
                information.Close();
                information.Archive();
                return false;
            }

            // Complete
            if (information.CreatorId == Identity.Id && information.State == InformationState.CLOSED)
            {
                await information.Compile();
                return false;
            }

            // Needs Work. Try to close it.
            if (information.State == InformationState.OPEN)
            {
                if (string.IsNullOrEmpty(information.AbilityName))
                {                    
                    // TODO: do we only pass abilities around?
                }
                else
                {
                    if (Abilities.ContainsKey(information.AbilityName))
                    {
                        await Execute(Abilities[information.AbilityName], information);
                    }
                    else
                    {
                        // Can't handle it. Return to creator.
                        information.Close($"No ability named {information.AbilityName} on {Name}.");
                        return false;
                    }
                }
                return true;
            }

            // Needs Analysis.Do something.
            if (information.State == InformationState.CLOSED)
            {
                await Review(information);
                return true;
            }

            throw new Exception("Unhandled Information");
        }

        //
       // public abstract Task Handle(InformationHandler information);
        public abstract Task Review(InformationHandler information);
        public abstract Task Execute(Ability ability, InformationHandler information);
        public abstract Task Compile(InformationHandler information);

        public async Task Execute(string abilityName, InformationHandler information)
        {
            if (Abilities.ContainsKey(abilityName))
            {
                await Execute(Abilities[abilityName], information);
            }
            else
            {
                // Can't handle it. Return to creator.
                //await SendToBroker(information, information.CreatorId);                
            }
        }

        public async Task Publish(InformationHandler information)
        {

            // Open drafts
            if (information.State == InformationState.DRAFT)
            {
                information.State = InformationState.OPEN;
            }

            // TODO: If this is an ability I can do, short circuit it here.

            // Forward to someone else
            if (information.OwnerId != Identity.Id)
            {
                await SendToBroker(information, information.OwnerId);
                return;
            }

            if (information.OwnerId == Identity.Id && information.State == InformationState.OPEN)
            {                
                information.Defer();
                return;
            }

            await SendToBroker(information, information.CreatorId);
        }

        public InformationHandler CreateInformation(string ability, string? input = null)
        {
            return InformationHandler.Create(this, Abilities[ability], input);
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

            SendStatusMessage($"Publish> {message.Information.ContextId} | {message.Information.AbilityName} | {message.Information.Input} | {message.Information.Output}");

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