using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime.Internal.Transform;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Technologai
{
    public class Auth
    {
        // TODO: Get from environment config
        private const int JWT_EXPIRY_SECONDS = 60 * 60 * 2;
        private readonly string _tableName = "TechnologaiDevAgentAuthKeys";
        private readonly string _secretKeySecretId = "mrk-c1a527a2856f4c98813d7642ea774e26";

        public async Task<APIGatewayHttpApiV2ProxyResponse> KeyGen(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            byte[] clientId = Array.Empty<byte>();
            JsonWebKey jsonWebKey = new();

            try
            {
                var keyGenRequest = JsonConvert.DeserializeObject<KeyGenRequest>(request.Body);

                clientId = Base64UrlEncoder.DecodeBytes(keyGenRequest?.ClientId);
                jsonWebKey = new JsonWebKey(keyGenRequest?.JsonWebKey);

                if (clientId.Length != 32) { throw new ArgumentException(nameof(clientId)); }
            }
            catch (Exception ex)
            {
#if DEBUG
                if (request.Body == "DEBUG")
                {
                    clientId = RandomNumberGenerator.GetBytes(32);
                    jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(RSA.Create(2048).ExportParameters(false)));
                    KeyGenRequest kgr = new KeyGenRequest
                    {
                        ClientId = Base64UrlEncoder.Encode(clientId),
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
            byte[] apiKey = clientId.Concat(clientSecret).ToArray();

            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "ClientSecretSaltHash", new AttributeValue { S = Base64UrlEncoder.Encode(clientSecretSaltHash) } },
                        { "Salt", new AttributeValue { S = Base64UrlEncoder.Encode(salt) } },
                        { "ClientId", new AttributeValue { S = Base64UrlEncoder.Encode(clientId) } },
                        { "CreatedDateTime", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                        { "CreatedBy", new AttributeValue { S = request.RequestContext.Authorizer.Jwt.Claims.TryGetValue("sub", out string? sub) ? sub : null }},
                        { "Active", new AttributeValue { BOOL = true } }
                    }
            };

            await new AmazonDynamoDBClient().PutItemAsync(putRequest);

#if DEBUG
            LambdaLogger.Log(Base64UrlEncoder.Encode(apiKey));
#endif

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(JwkToRsa(jsonWebKey));

                var keyGenResponse = new KeyGenResponse
                {
                    EncryptedApiKey = Base64UrlEncoder.Encode(rsa.Encrypt(apiKey, false))
                };

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(keyGenResponse)
                };
            }
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> Token(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            string clientId;
            byte[] clientSecret;

            try
            {
                var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(request.Body);

                byte[] apiKey = Base64UrlEncoder.DecodeBytes(tokenRequest?.ApiKey);
                byte[] clientIdBytes = new byte[32];

                clientSecret = new byte[32];

                Array.Copy(apiKey, clientIdBytes, 32);
                Array.Copy(apiKey, 32, clientSecret, 0, 32);

                clientId = Base64UrlEncoder.Encode(clientIdBytes);
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.Message);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Bad Request" })
                };
            }

            var dynamoDbClient = new AmazonDynamoDBClient();

            var querySaltRequest = new QueryRequest
            {
                TableName = _tableName,
                KeyConditionExpression = "ClientId = :clientId",
                FilterExpression = "Active = :active",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":clientId", new AttributeValue { S = clientId } },
                        { ":active", new AttributeValue { BOOL = true } }
                    },
                ProjectionExpression = "Salt,ClientSecretSaltHash"
            };

            QueryResponse querySaltResponse = await dynamoDbClient.QueryAsync(querySaltRequest);

            if (querySaltResponse.Items.Count != 1)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Bad Request" })
                };
            }

            byte[] salt = Base64UrlEncoder.DecodeBytes(querySaltResponse.Items[0]["Salt"].S);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            if (Base64UrlEncoder.Encode(clientSecretSaltHash) != querySaltResponse.Items[0]["ClientSecretSaltHash"].S)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "Bad Request" })
                };
            }

            var kms = new AmazonKeyManagementServiceClient();            

            var jwtHeader = new JwtHeader();
            jwtHeader.Add("alg", "PS256");
            jwtHeader.Add("typ", "JWT");

            var jwtPayload = new JwtPayload();
            jwtPayload.Add("sub", clientId);
            jwtPayload.Add("exp", Convert.ToString(DateTimeOffset.UtcNow.AddSeconds(JWT_EXPIRY_SECONDS).ToUnixTimeSeconds()));
            
            // TODO: Fill these
            jwtPayload.Add("kid", "");
            jwtPayload.Add("iss", "");
            jwtPayload.Add("aud", "");
            jwtPayload.Add("nbf", "");
            jwtPayload.Add("iat", "");
            jwtPayload.Add("scp", "");

            // https://docs.aws.amazon.com/apigateway/latest/developerguide/http-api-jwt-authorizer.html            
            // TODO: validate via jwks_uri

            string jwtHeaderBase64 = Base64UrlEncoder.Encode(jwtHeader.SerializeToJson());
            string jwtPayloadBase64 = Base64UrlEncoder.Encode(jwtPayload.SerializeToJson());

            var jwtSignRequest = new SignRequest
            {
                KeyId = _secretKeySecretId,
                SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256,
                MessageType = MessageType.RAW,
                Message = new MemoryStream(Encoding.UTF8.GetBytes($"{jwtHeaderBase64}.{jwtPayloadBase64}"))
            };

            SignResponse jwtSignResponse = await new AmazonKeyManagementServiceClient().SignAsync(jwtSignRequest);

            string jwtSignatureBase64 = Base64UrlEncoder.Encode(jwtSignResponse.Signature.ToArray());

            var tokenResponse = new TokenResponse
            {
                Token = $"{jwtHeaderBase64}.{jwtPayloadBase64}.{jwtSignatureBase64}"
            };

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(tokenResponse)
            };
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
    }

    public class KeyGenRequest
    {
        public string? ClientId { get; set; }
        public string? JsonWebKey { get; set; }
    }

    public class KeyGenResponse
    {
        public string? EncryptedApiKey { get; set; }
    }

    public class TokenRequest
    {
        public string? ApiKey { get; set; }
    }

    public class TokenResponse
    {
        public string? Token { get; set; }
    }

    public class MessageResponse
    {
        public string? Message { get; set; }
    }
}