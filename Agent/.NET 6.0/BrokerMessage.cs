using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class BrokerMessage
    {
        public string Topic { get { return $"{AgencyId ?? "-"}/{MemberId ?? "-"}"; } }
        public Information? Information { get; set; }
        public IProcess? Process { get; set; }
        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }
        public bool IsBroadcast { get { return MemberId?.Equals("0") ?? false; } }

        private BrokerMessage() { }

        internal static BrokerMessage FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split('/');

            var brokerMessage = new BrokerMessage()
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
            };

            if (brokerMessage.IsBroadcast)
            {
                brokerMessage.Process = Technologai.Process.FromJson(args.ApplicationMessage.ConvertPayloadToString());
            }
            else            
            {
                brokerMessage.Information = Information.FromJson(args.ApplicationMessage.ConvertPayloadToString());
            };

            return brokerMessage;
        }

        internal BrokerMessage(Identity identity)
        {
            AgencyId = identity.AgencyId;
        }
    }
}
