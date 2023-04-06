using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class Agent
    {
        public string Name { get; set; }

        private const string TOPIC = "my/topic";

        public event EventHandler<string>? OutputReceived;

        private MqttClient _mqtt;
        //private AppConfig _config = new AppConfig();

        private Authentication _auth = new Authentication();

        public Agent(string host, AgentIdentity agentIdentity)
        {
            _auth.Agent = agentIdentity;

            Name = _auth.Agent?.Name ?? "Unknown";

            _mqtt = new MqttClient(host, _auth);

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            Output(args.ApplicationMessage.ConvertPayloadToString());
        }

        public async Task Input(string message)
        {
            await _mqtt.PublishAsync(TOPIC, message);
        }

        private void Output(string message)
        {
            OutputReceived?.Invoke(this, message);
        }

        public async Task Start()
        {
            await _mqtt.ConnectAsync();
            Output($"{_auth.Agent?.Name ?? "Unknown"} Connected");

            await _mqtt.SubscribeAsync(TOPIC);
            Output($"{_auth.Agent?.Name ?? "Unknown"} Subscribed");
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}