using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.AWS.OpenID
{
    internal class Config
    {
        // TODO: Get from environment config
        internal const int JWT_EXPIRY_SECONDS = 60 * 60 * 2;
        internal const string CLIENT_TABLE_NAME = "TechnologaiDevAgentAuthKeys";

        internal static string SignatureKey = "mrk-c1a527a2856f4c98813d7642ea774e26";
        internal static string Issuer = "https://auth.technologai.com";
        internal static string AuthorizationEndpoint = "https://auth.technologai.com/authorize";
        internal static string TokenEndpoint = "https://auth.technologai.com/token";
        internal static string UserInfoEndpoint = "https://auth.technologai.com/userinfo";
        internal static string JwksUri = "https://auth.technologai.com/.well-known/jwks.json";
        internal static string RegistrationEndpoint = "https://auth.technologai.com/client";
        internal static List<string> GrantTypesSupported = new List<string>() {"client_credentials"};
        internal static List<string> ResponseTypesSupported = new List<string>() { "id_token"};
        internal static List<string> SubjectTypesSupported = new List<string>() { "public" };
        internal static List<string> IdTokenSigningAlgValuesSupported = new List<string>() { "RS256", "PS256" };
        internal static List<string> ScopesSupported = new List<string>() { "openid" };

        internal static string TokenAudience = "https://broker.technologai.com";


    }
}
