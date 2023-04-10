using Microsoft.IdentityModel.Tokens;
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
            ReceiveMessage(BrokerMessage.FromMqttArgs(args));            
        }

        private async Task<bool> PublishMessage(BrokerMessage message, Identity? identity)
        {
            if (identity == null || message.Information?.Payload == null)
            {
                return false;
            }

            string topic = identity.GetMaskedTopic(message.Topic);

            await _mqtt.PublishAsync(topic, message.Information.ToJson());

            SendStatusMessage($"Published to: {topic}");

            return true;
        }

        private async Task<bool> PublishMessage(BrokerMessage message)
        {
            if (Identity.AssignedRole?.Equals("agency") ?? false)
            {
                return await PublishMessage(message, Identity.Agency);
            }
            if (Identity.AssignedRole?.Equals("member") ?? false)
            {
                return await PublishMessage(message, Identity);
            }
            if (Identity.AssignedRole?.Contains("agent") ?? false)
            {
                return await PublishMessage(message, Identity.Agent);
            }
            return false;
        }

        public async Task<bool> PublishInformation(Information information)
        {
            var message = new BrokerMessage(Identity);
            message.Information = information;

            //_contexts.SetContextOrOwner(information);

            message.MemberId = information.OwnerId;

            return await PublishMessage(message);
        }

        private void ReceiveMessage(BrokerMessage message)
        {
            if (message.Information != null)
            {
                ReceiveInformation(message.Information);
            }
        }

        private void ReceiveInformation(Information information)
        {
            //_contexts.RecordContext(information);
            InformationReceived?.Invoke(this, information);
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

        public void NewContext(Information information)
        {
            _contexts.NewContext(information);            
        }
    }
}