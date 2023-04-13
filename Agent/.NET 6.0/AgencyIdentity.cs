using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public class AgencyIdentity : Identity
    {
        internal override TechnologaiRole RoleName => TechnologaiRole.agent;
        public AgentIdentity Agent { get; }
        internal override string PublishMask => $"{Id}/+/0/0";
        internal override string SubscribeMask => $"{Id}/0/0/0";

        internal AgencyIdentity(string id, AgentIdentity agent)
            : base(id)
        {
            this.Agent = agent;
        }
    }
}
