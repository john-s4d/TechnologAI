using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IdentityModel;


namespace Technologai
{
    public abstract class Identity
    {

        public Authority Authority { get; set; }
        public string? Name { get; set; }
        public string? AgentId { get; set; }
        public string? MemberId { get; set; }
        public string? AgencyId { get; set; }
        public string? Token { get; set; }
        public abstract string GrantType { get; }
        public abstract string Bearer { get; }
        public abstract string PublishMask { get; }
        public abstract string SubscribeMask { get; }

        public Identity(string name, Authority authority)
        {
            this.Name = name; 
            this.Authority = authority;
        }
    }
}
