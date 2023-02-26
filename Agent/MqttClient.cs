using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    internal class MqttClient
    {
        private string _host;
        private int _port;
        private string _username;
        private string _password;        

        private IMqttClient _client = new MqttFactory().CreateMqttClient();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal event EventHandler<MqttApplicationMessageReceivedEventArgs>? MessageReceived;        

        internal MqttClient(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;            
        }

        internal async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_host, _port)
                .WithCredentials(_username, _password)
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