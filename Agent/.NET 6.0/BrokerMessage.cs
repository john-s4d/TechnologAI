using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;

namespace Technologai
{
    public class BrokerMessage
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions();

        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }
        public AgentMessage? AgentMessage { get; set; }
        public string Topic { get { return $"{AgencyId ?? "-"}/{MemberId ?? "-"}"; } }        
        //public bool IsBroadcast { get { return MemberId?.Equals("0") ?? false; } }

        private BrokerMessage() { }

        internal BrokerMessage(Identity identity)
        {
            AgencyId = identity.AgencyId;
        }

        static BrokerMessage()
        {
            options.Converters.Add(new AgentMessageConverter());
        }

        internal static BrokerMessage FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split('/');

            var payload = args.ApplicationMessage.ConvertPayloadToString();

            return new BrokerMessage()
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
                AgentMessage = JsonSerializer.Deserialize<AgentMessage>(payload, options)
            };
        }

        internal string ConvertAgentMessageToString()
        {
            return JsonSerializer.Serialize(AgentMessage, options);
        }
    }
}
