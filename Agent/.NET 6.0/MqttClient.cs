using IdentityModel;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Security.Claims;

namespace Technologai
{
    internal class MqttClient
    {
        private const int PORT = 8083;

        private Identity _identity;

        private IMqttClient _client = new MqttFactory().CreateMqttClient();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public bool IsConnected => _client.IsConnected;
        
        private bool _isConnecting;

        internal event EventHandler<MqttApplicationMessageReceivedEventArgs>? MessageReceived;


        public MqttClient(Identity identity)
        {
            _identity = identity;
        }

        internal async Task ConnectAsync()
        {
            if (!_client.IsConnected && !_isConnecting)
            {
                _isConnecting = true;
                var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer($"{new Uri(_identity.Authority.BrokerUri).Host}:{PORT}")
                .WithTls()
                .WithCredentials(_identity.Tokens[_identity.Authority.BrokerUri], "password")
                .Build();

                _client.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;

                await _client.ConnectAsync(options, _cancellationTokenSource.Token);
                _isConnecting = false;
            }
        }

        private Task _client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            MessageReceived?.Invoke(this, arg);
            return Task.CompletedTask;
        }

        internal async Task SubscribeAsync(string subscribeMask)
        {
            if (!_client.IsConnected) { throw new InvalidOperationException("Not Connected"); }

            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(subscribeMask).Build(), _cancellationTokenSource.Token);
        }

        internal async Task DisconnectAsync()
        {
            _cancellationTokenSource.Cancel();
            await _client.DisconnectAsync();
            _client.Dispose();
        }

        internal async Task PublishAsync(string topic, string payload, bool retain = false, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (!_client.IsConnected)
            {
                await ConnectAsync();
            }

            if (_client.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel(qos)
                .Build();

                await _client.PublishAsync(message, _cancellationTokenSource.Token);

            }
        }
    }
}