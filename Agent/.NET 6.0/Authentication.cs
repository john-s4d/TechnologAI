using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    internal class Authentication
    {
        internal MemberIdentity? Agent { get; set; }
        internal Dictionary<string, MemberIdentity> Members { get; } = new Dictionary<string, MemberIdentity>();
        
    }
}
