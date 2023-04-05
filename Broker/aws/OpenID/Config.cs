using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.AWS.OpenID
{
    internal class Config
    {   
        internal const int JWT_EXPIRY_SECONDS = 60 * 60 * 2;
        internal const string CLIENT_TABLE_NAME = "TechnologaiDevAgentAuthKeys";
        internal const string SIGNATURE_KEY_ID = "mrk-c1a527a2856f4c98813d7642ea774e26";
    }
}
