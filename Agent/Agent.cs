using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class Agent
    {
        private const int PORT = 1883;
        private const string TOPIC = "my/topic";

        public event EventHandler<string>? Output;

        private MqttClient _mqtt;
        private AppConfig _config = new AppConfig();        
        
        public Agent(string host)
        {
            _mqtt = new MqttClient(host, PORT, _config.MqttUsername, _config.MqttPassword);
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