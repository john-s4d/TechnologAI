using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public class AgentIdentity : Identity
    {
        internal override string RoleName => "agent";
        public Authority Authority { get; }
        public AgentIdentity? SubAgent { get; }
        protected string ClientSecret { get;  }
        internal string Bearer => Base64UrlEncoder.Encode($"{this.Id}:{ClientSecret}");
        internal override string PublishMask => $"0/0/{this.Id}/+";
        internal override string SubscribeMask => $"0/0/{this.Id}/+";        

        internal AgentIdentity(string authorityName, string clientId, string clientSecret)
        {
            this.Authority = new Authority(authorityName);
            this.Id = clientId;
            this.ClientSecret = clientSecret;
        }
    }
}