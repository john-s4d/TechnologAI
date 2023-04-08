using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Technologai
{
    public class AgentIdentity : Identity
    {
        public string? ClientId { get; set; }
        internal string? ClientSecret { get; set; }
        public override string GrantType => "client_credentials";
        public override string Bearer => Base64UrlEncoder.Encode($"{ClientId}:{ClientSecret}");
        public override string PublishMask => $"0/0/0/{AgentId}/+";
        public override string SubscribeMask => $"0/0/0/{AgentId}/+";

        public AgentIdentity(string clientId, string clientSecret, Authority authority) 
            : base(string.Empty, authority)
        {   
            ClientId = clientId;
            ClientSecret = clientSecret;
            AgentId = clientId;
        }
    }
}