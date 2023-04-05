using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class Agent
    {
        private const string TOPIC = "my/topic";

        public event EventHandler<string>? Output;

        private MqttClient _mqtt;
        private AppConfig _config = new AppConfig();

        private Authentication _auth = new Authentication();

        public Agent(string host, AgentIdentity agentIdentity)
        {
            _auth.Agent = agentIdentity;

            _mqtt = new MqttClient(host, _auth);

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            Output?.Invoke(this, args.ApplicationMessage.ConvertPayloadToString());
        }

        public async Task Input(string message)
        {
            await _mqtt.PublishAsync(TOPIC, message);
        }

        public async Task Start()
        {   
            await _mqtt.ConnectAsync();
            await _mqtt.SubscribeAsync(TOPIC);
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}