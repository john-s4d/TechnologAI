using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using System.Net.Http;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public string? Name { get; private set; }

        public event EventHandler<Message>? MessageReceived;
        public event EventHandler<string>? StatusMessage;

        private MqttClient _mqtt;
        //private ContextProvider _context;

        public MemberIdentity Identity { get; }

        // TODO: Connect as an Agent, without memberId

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Identity = new MemberIdentity(memberId, new AgentIdentity(authorityName, clientId, clientSecret));

            //_context = new ContextProvider(Identity);

            _mqtt = new MqttClient(Identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            ReceiveMessage(Message.FromMqttArgs(args));
        }

        public async Task<bool> PublishMessage(Message message, Identity? identity)
        {
            if (identity == null) {  throw new ArgumentNullException(nameof(identity)); }

            string topic = identity.GetMaskedTopic(message.Topic);
            await _mqtt.PublishAsync(topic, message.Payload);
            //SendStatusMessage($"Published: {Identity.RoleName}:{topic}");

            return true;
        }

        public async Task<bool> PublishMessage(Message message)
        {
            if (Identity.AssignedRoles.Contains("agency"))
            {
                return await PublishMessage(message, Identity.Agency);
            }
            if (Identity.AssignedRoles.Contains("member"))
            {
                return await PublishMessage(message, Identity);
            }
            if (Identity.AssignedRoles.Contains("agent"))
            {
                return await PublishMessage(message, Identity.Agent);
            }
            return false;
        }
      

        private void ReceiveMessage(Message message)
        {
            MessageReceived?.Invoke(this, message);
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
                if (Identity.AssignedRoles.Contains(Identity.RoleName))
                {
                    await _mqtt.SubscribeAsync(Identity.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.RoleName}:{Identity.SubscribeMask}");
                }

                // Subscribe to Agency Topics
                if (Identity.Agency != null && Identity.AssignedRoles.Contains(Identity.Agency.RoleName))
                {
                    await _mqtt.SubscribeAsync(Identity.Agency.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agency.RoleName}:{Identity.Agency.SubscribeMask}");
                }

                // Subscribe to Agent Topics
                if (Identity.Agent != null && Identity.AssignedRoles.Contains(Identity.Agent.RoleName))
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

    }
}