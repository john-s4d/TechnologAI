using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public abstract class Identity
    {
        internal List<string> AssignedRoles { get; } = new List<string>();
        internal string? Name { get; set; }
        internal string? Id { get; set; }
        internal abstract string RoleName { get; }
        internal abstract string PublishMask { get; }
        internal abstract string SubscribeMask { get; }  
        
        
    }
}
