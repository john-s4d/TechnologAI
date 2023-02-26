using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class Agent
    {
        public event EventHandler<string>? Output;

        private MqttClient _mqtt;

        private const string USERNAME = "audiostream";
        private const string PASSWORD = "7snLBemg1T";
        private const int PORT = 1883;

        private const string TOPIC = "my/topic";

        public Agent(string host)
        {
            _mqtt = new MqttClient(host, PORT, USERNAME, PASSWORD);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {   
            Output?.Invoke(this, args.ApplicationMessage.ConvertPayloadToString());
        }

        public async void Publish(string message)
        {
            await _mqtt.PublishAsync(TOPIC, message);
        }

        public async void Start()
        {
            await _mqtt.ConnectAsync();
            await _mqtt.SubscribeAsync(TOPIC);
        }

        public async void Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}