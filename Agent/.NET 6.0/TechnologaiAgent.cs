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
        private Dictionary<ContextId, Information> _active { get; } = new();

        private MqttClient _mqtt;

        public Identity Identity { get; }

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Identity = new Identity(authorityName, clientId, clientSecret, memberId);
            Abilities = new AbilityCatalog(Identity);
            Context = new ContextProvider();

            _mqtt = new MqttClient(Identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        // ** TRANSPORT **

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
            Context.Add(information);

            if (information.State == InformationState.OPEN && !_active.ContainsKey(information.Id))
            {
                _active.Add(information.Id, information);
            }

            if (_active.ContainsKey(information.Id))
            {

                Information? contextInformation;

                if (information.State == InformationState.CLOSED && information.CreatorId == Identity.Id)
                {
                    contextInformation = Context.GetCreator(information.Id);
                }

                if (string.IsNullOrEmpty(information.Id))
                {
                    contextInformation = information;
                }

                if (await information.Assess())
                // TODO: Debounce
                // TODO: Probably need an assessment object
                {
                    await information.Execute();
                    await information.Publish();
                }
                else
                {
                    foreach (InformationAdapter item in await information.Spawn())
                    {
                        Context.Spawn(item.Id, information.Id);
                        await item.Publish();
                    }
                }
            }
        }

        protected internal abstract Task<bool> Assess(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext);
        protected internal abstract Task<Information> Execute(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext);
        protected internal abstract Task<List<Information>> Spawn(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext);

        public InformationAdapter Create(string abilityName, string? input = null)
        {
            var information = InformationAdapter.Create(this, abilityName, input);
            Context.Add(information);
            _active.Add(information.Id, information);
            return information;
        }

        protected internal void Close(InformationAdapter information)
        {
            _active.Remove(information.Id);
        }

        public async Task Publish(InformationAdapter information)
        {
            // Open drafts
            if (information.State == InformationState.DRAFT)
            {
                information.State = InformationState.OPEN;
            }

            SendStatusMessage($"{information.Id} Publish> {information.AbilityName} | {information.Input} | {information.Output}");

            // TODO: short circuit.
            /*
            if (Identity.Id == information.OwnerId)
            {
                Receive(information);
                return Task.CompletedTask;
            }*/

            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = information.OwnerId
            };

            string? topic = Identity.GetMaskedTopic(message.TopicMember);

            await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

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