using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    internal class AgentIdentity : Identity
    {
        internal override string RoleName => "agent";
        internal Authority Authority { get; }
        protected string ClientSecret { get;  }
        internal string Bearer => Base64UrlEncoder.Encode($"{this.Id}:{ClientSecret}");
        internal override string PublishMask => $"0/0/0/{this.Id}/+";
        internal override string SubscribeMask => $"0/0/0/{this.Id}/+";        

        internal AgentIdentity(string authorityName, string clientId, string clientSecret)
        {
            this.Authority = new Authority(authorityName);
            this.Id = clientId;
            this.ClientSecret = clientSecret;
        }
    }
}