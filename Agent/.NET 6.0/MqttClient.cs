using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    internal class MqttClient
    {
        private const int PORT = 8083;

        private string _host;                      

        private IMqttClient _client = new MqttFactory().CreateMqttClient();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Authentication _authentication;

        internal event EventHandler<MqttApplicationMessageReceivedEventArgs>? MessageReceived;        

        internal MqttClient(string host, Authentication authentication)
        {
            _host = string.IsNullOrEmpty(host) ? throw new ArgumentNullException(nameof(host)) : host;
            _authentication = authentication == null ? throw new ArgumentNullException(nameof(authentication)) : authentication;
        }

        internal async Task ConnectAsync()
        {
            if (!_client.IsConnected && _authentication.Agent != null)
            {   
               await _authentication.Agent.Authenticate();

                var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer($"{_host}:{PORT}")
                .WithTls()
                .WithCredentials(_authentication.Agent.Token, "password")
                .Build();

                _client.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;

                await _client.ConnectAsync(options, _cancellationTokenSource.Token);                             
            }
        }

        internal async Task SubscribeAsync(string topic)
        {
            if (!_client.IsConnected) { throw new InvalidOperationException("Not Connected"); }

            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build(), _cancellationTokenSource.Token);            
        }

        private async Task _client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            await Task.Run(() => MessageReceived?.Invoke(this, args));
        }

        internal async Task DisconnectAsync()
        {
            _cancellationTokenSource.Cancel();
            await _client.DisconnectAsync();
            _client.Dispose();
        }

        internal async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 0)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                .Build();

            await _client.PublishAsync(message, _cancellationTokenSource.Token);
        }
    }
}