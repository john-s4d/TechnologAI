using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    internal class AgencyIdentity : Identity
    {
        internal override string RoleName => "agency";
        internal AgentIdentity Agent { get; }       
        internal override string PublishMask => $"{this.Id}/+/+/0/0";
        internal override string SubscribeMask => $"{this.Id}/0/+/0/0";

        internal AgencyIdentity(string id, AgentIdentity agent)
        {
            this.Id = id;
            this.Agent = agent;
        }
    }
}
