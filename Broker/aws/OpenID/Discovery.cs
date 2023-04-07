using Amazon.KeyManagementService.Model;
using Amazon.KeyManagementService;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Technologai.AWS.OpenID
{
    public class OpenIDDiscoveryController
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> DiscoveryGet(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            return new DiscoverySuccessResponse(200, new OpenIDConfiguration());
        }

        public class OpenIDConfiguration
        {
            public string issuer { get; set; } = Config.Issuer;
            public string jwks_uri { get; set; } = Config.JwksUri;
            public string registration_endpoint { get; set; } = Config.RegistrationEndpoint;
            //public string pushed_authorization_request_endpoint { get; set; } = Config.AuthorizationEndpoint;
            public string authorization_endpoint { get; set; } = Config.AuthorizationEndpoint;
            public string token_endpoint { get; set; } = Config.TokenEndpoint;
            //public string introspection_endpoint { get; set; } = Config.IntrospectionEndpoint;
            //public string revocation_endpoint { get; set; } = Config.RevocationEndpoint;            
            public string userinfo_endpoint { get; set; } = Config.UserInfoEndpoint;            
            public List<string> grant_types_supported { get; set; } = Config.GrantTypesSupported;
            public List<string> response_types_supported { get; set; } = Config.ResponseTypesSupported;
            //public List<string> response_modes_supported { get; set; } = Config.;
            //public List<string> prompt_values_supported { get; set; } = Config.;
            //public List<string> code_challenge_methods_supported { get; set; } = Config.;
            //public List<string> token_endpoint_auth_methods_supported { get; set; } = Config.;
            //public List<string> id_token_signing_alg_values_supported { get; set; } = Config.;
            //public List<string> userinfo_signing_alg_values_supported { get; set; } = Config.;
            //public List<string> subject_types_supported { get; set; } = Config.;
            //public List<string> acr_values_supported { get; set; } = Config.;
            //public List<string> display_values_supported { get; set; } = Config.;
            //public List<string> scopes_supported { get; set; } = Config.;
            //public List<string> claim_types_supported { get; set; } = Config.;
            //public List<string> claims_supported { get; set; } = Config.;
            //public List<string> claims_parameter_supported { get; set; } = Config.;
            //public List<string> request_parameter_supported { get; set; } = Config.;
            //public List<string> request_uri_parameter_supported { get; set; } = Config.;
            //public List<string> require_request_uri_registration  { get; set; } = Config.;
            //public List<string> request_uri_quota  { get; set; } = Config.;
            //public List<string> request_uri_parameter_supported { get; set; } = Config.;
        }

        public class DiscoverySuccessResponse : APIGatewayHttpApiV2ProxyResponse
        {
            public DiscoverySuccessResponse(int statusCode, OpenIDConfiguration body)
            {
                this.StatusCode = statusCode;
                Body = JsonSerializer.Serialize(body);
            }
        }
    }    
}
