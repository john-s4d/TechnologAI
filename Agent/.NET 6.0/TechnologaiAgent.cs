using MQTTnet;
using MQTTnet.Client;
using System.Net.Http;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public string? Name { get; private set; }

        public event EventHandler<Information>? InformationReceived;
        public event EventHandler<string>? StatusMessage;

        private MqttClient _mqtt;
        private ContextProvider _contexts;

        public MemberIdentity Identity { get; } // TODO: Connect as an Agent, without memberId

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Identity = new MemberIdentity(memberId, new AgentIdentity(authorityName, clientId, clientSecret));

            _contexts = new ContextProvider(Identity);
            _mqtt = new MqttClient(Identity);

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            _ = Receive(BrokerMessage.FromMqttArgs(args).Information);
        }

        private async Task Receive(Information? information)
        {
            if (information == null) { return; }

            if (information.CreatorId == Identity.Id)
            { 
                if (information.State == InformationState.OPEN)
                {
                    // It came back. Try something else. Don't send it out again as is though.                    
                }
                if (information.State == InformationState.INCOMPLETE)
                {
                    // It came back partially done. Either keep going or wrap it up.
                }
            }

            if (information.State == InformationState.COMPLETE && (information.CreatorId == Identity.Id))
            {
                _contexts.MarkComplete(information);
            }
            else if (information.State == InformationState.COMPLETE && (information.CreatorId != Identity.Id))
            {
                await Publish(information, information.CreatorId); // It's complete but not for this agent. Forward it along.
                return;
            }

            InformationReceived?.Invoke(this, information);
        }

        public async Task<bool> Publish(Information information, string? memberId = null)
        {
            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = memberId
            };

            return await Publish(message);
        }

        private async Task<bool> Publish(BrokerMessage message)
        {
            if (message.Information == null)
            {
                return false;
            }

            Identity? publishIdentity = null;

            if ((Identity.Agency != null) && (Identity.AssignedRole?.Equals("agency") ?? false))
            {
                publishIdentity = Identity.Agency;
            }
            if (Identity.AssignedRole?.Equals("member") ?? false)
            {
                publishIdentity = Identity;
            }
            if (Identity.AssignedRole?.Contains("agent") ?? false)
            {
                publishIdentity = Identity.Agent;
            }

            if (publishIdentity != null)
            {
                string? topic = publishIdentity?.GetMaskedTopic(message.Topic);

                await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

                SendStatusMessage($"Published: {message.Information.ContextId} {topic}");

                return true;
            }

            return false;
        }

        private void SendStatusMessage(string message)
        {
            StatusMessage?.Invoke(this, message);
        }

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

                // Subscribe to Member Topics
                if (Identity.AssignedRole?.Equals(Identity.RoleName) ?? false)
                {
                    await _mqtt.SubscribeAsync(Identity.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.RoleName}:{Identity.SubscribeMask}");
                }

                // Subscribe to Agency Topics
                if (Identity.Agency != null && (Identity.AssignedRole?.Equals(Identity.Agency.RoleName) ?? false))
                {
                    await _mqtt.SubscribeAsync(Identity.Agency.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agency.RoleName}:{Identity.Agency.SubscribeMask}");
                }

                // Subscribe to Agent Topics
                if (Identity.Agent != null && (Identity.AssignedRole?.Equals(Identity.Agent.RoleName) ?? false))
                {
                    await _mqtt.SubscribeAsync(Identity.Agent.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agent.RoleName}:{Identity.Agent.SubscribeMask}");
                }

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

        public Information Spawn(Information information, InformationState state = InformationState.OPEN, string? input = null)
        {
            return _contexts.Spawn(information, state, input);
        }

        public Information CreateInformation(string input)
        {
            return _contexts.CreateInformation(input);
        }
    }
}