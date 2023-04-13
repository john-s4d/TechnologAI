using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public class AgentIdentity : Identity
    {
        internal override TechnologaiRole RoleName => TechnologaiRole.agent;
        public Authority Authority { get; }
        public AgentIdentity? SubAgent { get; }
        protected string ClientSecret { get;  }
        internal string Bearer => Base64UrlEncoder.Encode($"{Id}:{ClientSecret}");
        internal override string PublishMask => $"0/0/{Id}/+";
        internal override string SubscribeMask => $"0/0/{Id}/+";

        internal AgentIdentity(string authorityName, string clientId, string clientSecret) 
            : base(clientId)
        {
            this.Authority = new Authority(authorityName);        
            this.ClientSecret = clientSecret;
        }
    }
}