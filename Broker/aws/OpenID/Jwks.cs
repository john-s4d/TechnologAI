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

namespace Technologai.AWS.OpenID
{
    internal class Jwks
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> GetJwks(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            GetPublicKeyRequest publicKeyRequest = new GetPublicKeyRequest
            {
                KeyId = Config.SIGNATURE_KEY_ID
            };

            GetPublicKeyResponse publicKeyResponse = await new AmazonKeyManagementServiceClient().GetPublicKeyAsync(publicKeyRequest);

            var rsaPublicKey = RSA.Create();

            rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyResponse.PublicKey.ToArray(), out int bytesRead);

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(rsaPublicKey)
            {
                KeyId = Config.SIGNATURE_KEY_ID
            });

            jwk.Use = "sig";

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = JsonExtensions.SerializeToJson(new JwksResponse { keys = { jwk } }) // JsonExtensions does a better job of serializing jwk
            };
        }
    }
}
