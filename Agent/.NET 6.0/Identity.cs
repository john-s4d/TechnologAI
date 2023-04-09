using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public abstract class Identity
    {          
        internal string? Name { get; set; }
        internal string? Id { get; set; }        
        internal abstract string PublishMask { get; }
        internal abstract string SubscribeMask { get; }      
    }
}
