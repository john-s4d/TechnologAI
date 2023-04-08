using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace Technologai
{
    public class MemberIdentity : Identity
    {
        public AgentIdentity Agent { get; set; }
        public AgencyIdentity? Agency { get; set; }

        public override string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
        public override string Bearer => Agent?.Token ?? string.Empty;
        public override string PublishMask => $"{AgencyId}/0/+/0/0";
        public override string SubscribeMask => $"{AgencyId}/{MemberId}/+/0/0";

        public MemberIdentity(string memberId, AgentIdentity agent, AgencyIdentity? agency = null)
            : base(string.Empty, agent.Authority)
        {
            MemberId = memberId;
            Agent = agent;
            Agency = agency;
        }

        internal async Task Authenticate()
        {
            await Authority.Authenticate(Agent);

            if (Agency != null) { await Authority.Authenticate(Agency); }

            await Authority.Authenticate(this);
        }
    }
}
