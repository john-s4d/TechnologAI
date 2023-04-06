using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.AWS.OpenID
{
    internal class Registration
    {
        // TODO: Provide method to deactivate ClientId

        public async Task<APIGatewayHttpApiV2ProxyResponse> ClientPost(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonConvert.SerializeObject(request)}");
            LambdaLogger.Log($"context: {JsonConvert.SerializeObject(context)}");


            if (request.RequestContext.Authorizer == null)
            {
                LambdaLogger.Log("No Authorizer");

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Unauthorized" })
                };
            }
            else
            {
                // TODO: Do we need to validate Authorizer subject? Ideally issuer trust should be enough.
            }

            byte[] clientIdBytes = Array.Empty<byte>();
            JsonWebKey jsonWebKey = new();

            try
            {
                var keyGenRequest = JsonConvert.DeserializeObject<ClientPostRequest>(request.Body);

                clientIdBytes = Base64UrlEncoder.DecodeBytes(keyGenRequest?.ClientId);
                jsonWebKey = new JsonWebKey(keyGenRequest?.JsonWebKey);

                if (clientIdBytes.Length != 32) { throw new ArgumentException(nameof(ClientPostRequest.ClientId)); }
            }
            catch (Exception ex)
            {
#if DEBUG
                if (request.Body == "DEBUG")
                {
                    clientIdBytes = RandomNumberGenerator.GetBytes(32);
                    jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(RSA.Create(2048).ExportParameters(false)));
                    var kgr = new ClientPostRequest
                    {
                        ClientId = Base64UrlEncoder.Encode(clientIdBytes),
                        JsonWebKey = JsonExtensions.SerializeToJson(jsonWebKey)
                    };
                    LambdaLogger.Log(JsonConvert.SerializeObject(kgr));
                }
#else
                LambdaLogger.Log(ex.Message);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Bad Request" })
                };
#endif
            }

            byte[] clientSecret = RandomNumberGenerator.GetBytes(32);
            byte[] salt = RandomNumberGenerator.GetBytes(32);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            string clientId = Base64UrlEncoder.Encode(clientIdBytes);

            var putRequest = new PutItemRequest
            {
                TableName = Config.CLIENT_TABLE_NAME,
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "ClientSecretSaltHash", new AttributeValue { S = Base64UrlEncoder.Encode(clientSecretSaltHash) } },
                        { "Salt", new AttributeValue { S = Base64UrlEncoder.Encode(salt) } },
                        { "ClientId", new AttributeValue { S = clientId } },
                        { "CreatedDateTime", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                        { "CreatedBy", new AttributeValue { S = request.RequestContext.Authorizer != null ? request.RequestContext.Authorizer.Jwt.Claims["sub"] : string.Empty } },
                        { "Active", new AttributeValue { BOOL = true } }
                    },
                ConditionExpression = "attribute_not_exists(ClientId)"

            };

            try
            {
                await new AmazonDynamoDBClient().PutItemAsync(putRequest);
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 500,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Could not save key" })
                };
            }

#if DEBUG   
            LambdaLogger.Log($"clientId: {clientId}");
            LambdaLogger.Log($"clientSecret: {Base64UrlEncoder.Encode(clientSecret)}");
#endif

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(JwkToRsa(jsonWebKey));

                var keyGenResponse = new ClientPostResponse
                {   
                    EncryptedClientSecret = Base64UrlEncoder.Encode(rsa.Encrypt(clientSecret, false))
                };

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(keyGenResponse)
                };
            }
        }
        private static RSAParameters JwkToRsa(JsonWebKey jwk)
        {
            RSAParameters rsa = new RSAParameters();
            rsa.Exponent = string.IsNullOrEmpty(jwk.E) ? null : Base64UrlEncoder.DecodeBytes(jwk.E);
            rsa.Modulus = string.IsNullOrEmpty(jwk.N) ? null : Base64UrlEncoder.DecodeBytes(jwk.N);
            rsa.D = string.IsNullOrEmpty(jwk.D) ? null : Base64UrlEncoder.DecodeBytes(jwk.D);
            rsa.DP = string.IsNullOrEmpty(jwk.DP) ? null : Base64UrlEncoder.DecodeBytes(jwk.DP);
            rsa.DQ = string.IsNullOrEmpty(jwk.DQ) ? null : Base64UrlEncoder.DecodeBytes(jwk.DQ);
            rsa.P = string.IsNullOrEmpty(jwk.P) ? null : Base64UrlEncoder.DecodeBytes(jwk.P);
            rsa.Q = string.IsNullOrEmpty(jwk.Q) ? null : Base64UrlEncoder.DecodeBytes(jwk.Q);
            rsa.InverseQ = string.IsNullOrEmpty(jwk.QI) ? null : Base64UrlEncoder.DecodeBytes(jwk.QI);
            return rsa;
        }

        // TODO: Use RFC request/response types

        public class ClientPostRequest
        {
            public string? ClientId { get; set; }
            public string? JsonWebKey { get; set; }
        }

        public class ClientPostResponse
        {
            public string? EncryptedClientSecret { get; set; }
        }

        public class MessageResponse
        {
            public string? Message { get; set; }
        }
    }
}
