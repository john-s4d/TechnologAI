using MQTTnet;
using MQTTnet.Client;
using System.Security.Authentication;

namespace Technologai
{
    public class AgencyMember
    {
        public string Name { get; set; } = string.Empty;

        public event EventHandler<string>? OutputReceived;

        private MqttClient _mqtt;        

        private MemberIdentity _identity;        

        public AgencyMember(MemberIdentity identity)
        {
            _identity = identity;

            _mqtt = new MqttClient(_identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            Output(args.ApplicationMessage.ConvertPayloadToString());
        }

        public async Task Input(string message, string context = "0")
        {
            await _mqtt.PublishAsync($"{_identity.AgencyId}/0/{context}/0/0", message);
        }

        private void Output(string message)
        {
            OutputReceived?.Invoke(this, message);
        }

        public async Task Start()
        {
            try { 
            await _identity.Authenticate();
            Output($"{_identity.Name} Authenticated");

            await _mqtt.ConnectAsync();
            Output($"{_identity.Name} Connected");

            await _mqtt.SubscribeAsync(_identity.SubscribeMask);
            Output($"{_identity.Name} Subscribed");

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