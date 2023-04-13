using MQTTnet;
using MQTTnet.Client;

namespace Technologai
{
    public class BrokerMessage
    {
        public string Topic => $"{AgencyId}/{MemberId}/{AgentId}/{SubagentId}";
        public Information? Information { get; set; }
        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }
        public string? AgentId { get; set; }
        public string? SubagentId { get; set; }

        private BrokerMessage() { }

        internal static BrokerMessage FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split('/');

            return new BrokerMessage
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
                AgentId = topicParts[2],
                SubagentId = topicParts[3],
                Information = Information.FromJson(args.ApplicationMessage.ConvertPayloadToString())
            };
        }

        internal BrokerMessage(MemberIdentity identity)
        {
            AgencyId = identity.Agency?.Id ?? throw new ArgumentNullException(nameof(identity.Agency));
            AgentId = identity.Agent.Id;
        }

        internal BrokerMessage(AgencyIdentity identity)
        {
            AgencyId = identity.Id;
            AgentId = identity.Agent.Id;
        }

        internal BrokerMessage(AgentIdentity identity)
        {
            AgentId = identity.Id;
            SubagentId = identity.SubAgent?.Id;
        }
    }
}
