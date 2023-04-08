using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Technologai
{
    public class AgencyIdentity : Identity
    {
        public AgentIdentity Agent { get; set; }

        public override string Bearer => Agent.Token ?? string.Empty;
        public override string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";

        public override string PublishMask => $"{AgencyId}/0/+/0/0";
        public override string SubscribeMask => $"{AgencyId}/+/+/0/0";

        public AgencyIdentity(string agencyId, AgentIdentity agent) 
            : base(string.Empty, agent.Authority)
        {
            AgencyId = agencyId;
            Agent = agent;
        }
    }
}
