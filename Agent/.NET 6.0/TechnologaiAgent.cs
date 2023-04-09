using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public string Name { get; set; } = string.Empty;

        public event EventHandler<string>? OutputReceived;

        private MqttClient _mqtt;

        private MemberIdentity _identity;

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            _identity = new MemberIdentity(memberId, new AgentIdentity(authorityName, clientId, clientSecret));

            _mqtt = new MqttClient(_identity);

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            Output(args.ApplicationMessage.ConvertPayloadToString());
        }

        public async Task Publish(string message, string context = "0")
        {
            if (context.Contains("+") || context.Contains("/"))
            {
                throw new ArgumentException(nameof(context));
            }

            string topic = _identity.PublishMask.Replace("+", context); // TODO: Publish according to the roles

            await _mqtt.PublishAsync(topic, message);
            Output($"Published: {topic}");
        }

        private void Output(string message)
        {
            OutputReceived?.Invoke(this, message);
        }

        public async Task Start()
        {
            try
            {
                await _identity.Authenticate();
                Output($"{_identity.Name} Authenticated");

                await _mqtt.ConnectAsync();
                Output($"{_identity.Name} Connected");

                await _mqtt.SubscribeAsync(_identity.SubscribeMask);
                Output($"Subscribed: {_identity.SubscribeMask}");

                await _mqtt.SubscribeAsync(_identity.Agency?.SubscribeMask ?? string.Empty); // TODO: Subscribe according to the roles
                Output($"Subscribed: {_identity.Agency?.SubscribeMask ?? string.Empty}");



            }
            catch (Exception ex)
            {
                Output(ex.ToString());
            }
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }

    }
}