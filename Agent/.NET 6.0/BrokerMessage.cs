using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class BrokerMessage
    {
        public string Topic => $"{AgencyId ?? "0"}/{MemberId ?? "0"}/{AgentId ?? "0"}/{SubagentId ?? "0"}";
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
            AgencyId = identity.Agency?.Id;
            AgentId = identity.Agent.Id;
            MemberId = identity.Id;            
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
