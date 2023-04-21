using IdentityModel;
using MQTTnet;
using MQTTnet.Client;
using System.Security.Claims;

namespace Technologai
{
    internal class MqttClient
    {
        private const int PORT = 8083;

        private Identity _identity;

        private IMqttClient _client = new MqttFactory().CreateMqttClient();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal event EventHandler<MqttApplicationMessageReceivedEventArgs>? MessageReceived;

   
        public MqttClient(Identity identity)
        {
            _identity = identity;
        }

        internal async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer($"{_identity.Authority.BrokerHost}:{PORT}")
                .WithTls()
                .WithCredentials(_identity.Token, "password")
                .Build();

                _client.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;                

                await _client.ConnectAsync(options, _cancellationTokenSource.Token);
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