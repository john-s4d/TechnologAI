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
        internal static readonly int JwtExpirySeconds = 60 * 60 * 24; // TODO: Currently 24 hours. Probably should be less.
        internal static readonly string SalesforceApiSecretArn = "arn:aws:secretsmanager:us-east-1:154032908746:secret:technologai/salesforce/api-EfjTYM";
        internal static readonly int SfdcSessionExpiryMinutes = 60;
        //internal static readonly string AwsSecretsRegion = "us-east-1";
        internal static readonly string AWSClientTableName = "TechnologaiDevAgentAuthKeys";
        internal static readonly string SignatureKey = "mrk-c1a527a2856f4c98813d7642ea774e26";
        internal static readonly string Issuer = "https://auth.technologai.com";
        internal static readonly string AuthorizationEndpoint = "https://auth.technologai.com/authorize";
        internal static readonly string TokenEndpoint = "https://auth.technologai.com/token";
        internal static readonly string UserInfoEndpoint = "https://auth.technologai.com/userinfo";
        internal static readonly string JwksUri = "https://auth.technologai.com/.well-known/jwks.json";
        internal static readonly string RegistrationEndpoint = "https://auth.technologai.com/client";
        internal static readonly List<string> GrantTypesSupported = new List<string>() {"client_credentials"};
        internal static readonly List<string> ResponseTypesSupported = new List<string>() { "id_token"};
        //internal static readonly List<string> SubjectTypesSupported = new List<string>() { "public" };
        //internal static readonly List<string> IdTokenSigningAlgValuesSupported = new List<string>() { "RS256", "PS256" };
        //internal static readonly List<string> ScopesSupported = new List<string>() { "openid" };
        internal static readonly string BrokerUri = "https://broker.technologai.com";
        internal static readonly string StreamUri = "https://stream.technologai.com";

    }
}
