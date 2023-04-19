using MQTTnet.Client;

namespace Technologai
{
    public abstract class TechnologaiAgent
    {
        public event EventHandler<string>? StatusMessage;

        public string? Name { get; private set; }
        public AbilityCatalog Abilities { get; private set; }
        internal ContextProvider Context { get; private set; }
        private Dictionary<string, Information> _active { get; } = new();

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
                Receive(new InformationAdapter(this, information)).Wait();
            }
        }

        private async Task Receive(InformationAdapter information)
        {
            Context.Add(information);

            if (information.State == InformationState.OPEN && !_active.ContainsKey(information.ContextId))
            {
                _active[information.ContextId] = information;
            }

            if (_active.ContainsKey(information.ContextId))
            {
                if (information.State == InformationState.CLOSED && information.CreatorId == Identity.Id)
                {
                    _active.Remove(information.ContextId);

                    var creator = Context.GetCreator(information.ContextId);
                    if (creator != null)
                    {
                        information = new InformationAdapter(this, information);
                    }
                }

                var assessment = (await information.Assess());

                if (assessment.Result == AssessmentResult.EXECUTE)
                {
                    // TODO: Debounce
                    await information.Execute(assessment);
                    await information.Publish();
                }

                else
                {
                    foreach (InformationAdapter item in await information.Spawn(assessment))
                    {
                        Context.Spawn(item.ContextId, information.ContextId);
                        await item.Publish();
                    }
                }
            }
        }

        public InformationAdapter Create(string abilityName, string? input = null)
        {
            var information = InformationAdapter.Create(this, abilityName, input);
            Context.Add(information);
            _active[information.ContextId] = information;
            return information;
        }

        public async Task Publish(InformationAdapter information)
        {
            // Open drafts
            information.OpenDrafts();

            SendStatusMessage($"{information.ContextId} Publish> {information.AbilityId} | {information.Input} | {information.Output}");

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
                MemberId = information.WorkerId
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