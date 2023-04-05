using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.KeyManagementService.Model;
using Amazon.KeyManagementService;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Technologai.AWS.OpenID
{
    internal class Token
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> PostToken(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonConvert.SerializeObject(request)}");
            LambdaLogger.Log($"context: {JsonConvert.SerializeObject(context)}");

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
                TableName = Config.CLIENT_TABLE_NAME,
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
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "ApiKey Not Found" })
                };
            }

            byte[] salt = Base64UrlEncoder.DecodeBytes(querySaltResponse.Items[0]["Salt"].S);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            string inputClientSecretSaltHash = Base64UrlEncoder.Encode(clientSecretSaltHash);
            string dbClientSecretSaltHash = querySaltResponse.Items[0]["ClientSecretSaltHash"].S;

            if (inputClientSecretSaltHash != dbClientSecretSaltHash)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonConvert.SerializeObject(new MessageResponse { Message = "ApiKey Not Verified" })
                };
            }

            var kms = new AmazonKeyManagementServiceClient();

            var jwtHeader = new JwtHeader();
            jwtHeader.Add("alg", "PS256");
            jwtHeader.Add("typ", "JWT");

            var jwtPayload = new JwtPayload();
            jwtPayload.Add("sub", clientId);
            jwtPayload.Add("exp", Convert.ToString(DateTimeOffset.UtcNow.AddSeconds(Config.JWT_EXPIRY_SECONDS).ToUnixTimeSeconds()));
            jwtPayload.Add("iat", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            jwtPayload.Add("nbf", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            jwtPayload.Add("kid", Config.SIGNATURE_KEY_ID);
            jwtPayload.Add("iss", "");

            //jwtPayload.Add("aud", "");
            //jwtPayload.Add("scp", "");


            string jwtHeaderBase64 = Base64UrlEncoder.Encode(jwtHeader.SerializeToJson());
            string jwtPayloadBase64 = Base64UrlEncoder.Encode(jwtPayload.SerializeToJson());

            var jwtSignRequest = new SignRequest
            {
                KeyId = Config.SIGNATURE_KEY_ID,
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

        public class TokenRequest
        {
            public string? ApiKey { get; set; }
        }

        public class TokenResponse
        {
            public string? Token { get; set; }
        }
    }
}
