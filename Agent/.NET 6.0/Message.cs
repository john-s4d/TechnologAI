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
    public class Message
    {
        public string Topic => $"{AgencyId ?? "0"}/{MemberId ?? "0"}/{AgentId ?? "0"}/{SubagentId ?? "0"}";
        public string Payload { get; set; } = string.Empty;
        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }
        public Context? Context { get; set; }
        public string? AgentId { get; set; }
        public string? SubagentId { get; set; }

        private Message() { }

        internal static Message FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split('/');

            return new Message
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
                AgentId = topicParts[2],
                SubagentId = topicParts[3],
                Payload = args.ApplicationMessage.ConvertPayloadToString()                
            };
        }

        public Message(MemberIdentity identity)
        {   
            AgencyId = identity.Agency?.Id;
            AgentId = identity.Agent.Id;
            MemberId = identity.Id;
        }
        public Message(AgencyIdentity identity)
        {
            AgencyId = identity.Id;            
            AgentId = identity.Agent.Id;
            MemberId = identity.Id;
        }
        public Message(AgentIdentity identity)
        {   
            AgentId = identity.Id;            
        }
    }
}
