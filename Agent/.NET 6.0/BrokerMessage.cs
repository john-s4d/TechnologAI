using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class BrokerMessage
    {
        public string TopicMember => $"{AgencyId}/{MemberId}";
        public string TopicAgency => $"{AgencyId}/0";
        public Information? Information { get; set; }
        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }

        private BrokerMessage() { }

        internal static BrokerMessage FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split('/');

            return new BrokerMessage
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
                Information = Information.FromJson(args.ApplicationMessage.ConvertPayloadToString())
            };
        }

        internal BrokerMessage(Identity identity)
        {
            AgencyId = identity.AgencyId;
            MemberId = identity.Id;
        }        
    }
}
