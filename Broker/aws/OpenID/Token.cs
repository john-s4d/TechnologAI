using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.KeyManagementService.Model;
using Amazon.KeyManagementService;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Util;
using System.Net.Mime;

namespace Technologai.AWS.OpenID
{
    internal class Token
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> TokenPost(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            string clientId;
            string clientSecret;

            try
            {
                var headers = new Dictionary<string, string>(request.Headers, StringComparer.OrdinalIgnoreCase);

                if (!headers.ContainsKey(HeaderKeys.ContentTypeHeader) || !headers[HeaderKeys.ContentTypeHeader].StartsWith("application/json"))
                {
                    return new TokenErrorResponse(415, "Unsupported Media Type");
                }

                var tokenRequest = JsonSerializer.Deserialize<TokenRequest>(request.Body) ?? throw new ArgumentNullException(nameof(request.Body));

                if (tokenRequest.grant_type != "client_credentials")
                {
                    return new TokenErrorResponse(400, "unsupported_grant_type");
                }

                string authHeader = headers[HeaderKeys.AuthorizationHeader] ?? throw new ArgumentNullException(nameof(request.Body));
                string[] credentials = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(authHeader.Substring("Basic ".Length).Trim())).Split(':');

                clientId = credentials[0];
                clientSecret = credentials[1];

            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.Message);
                return new TokenErrorResponse(400, "Bad Request");
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
                return new TokenErrorResponse(401, "invalid_client");
            }

            byte[] salt = Base64UrlEncoder.DecodeBytes(querySaltResponse.Items[0]["Salt"].S);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(Base64UrlEncoder.DecodeBytes(clientSecret).Concat(salt).ToArray());

            string inputClientSecretSaltHash = Base64UrlEncoder.Encode(clientSecretSaltHash);
            string dbClientSecretSaltHash = querySaltResponse.Items[0]["ClientSecretSaltHash"].S;

            if (inputClientSecretSaltHash != dbClientSecretSaltHash)
            {
                return new TokenErrorResponse(401, "invalid_client");
            }

            var kms = new AmazonKeyManagementServiceClient();

            var jwtHeader = new JwtHeader();
            jwtHeader.Add("alg", "RS256");
            jwtHeader.Add("typ", "JWT");
            jwtHeader.Add("kid", Config.SignatureKey);

            var jwtPayload = new JwtPayload();
            jwtPayload.Add("sub", clientId);
            jwtPayload.Add("exp", Convert.ToString(DateTimeOffset.UtcNow.AddSeconds(Config.JWT_EXPIRY_SECONDS).ToUnixTimeSeconds()));
            jwtPayload.Add("iat", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            jwtPayload.Add("nbf", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));            
            jwtPayload.Add("iss", Config.Issuer);
            jwtPayload.Add("aud", Config.TokenAudience);

            //jwtPayload.Add("scp", "");


            string jwtHeaderBase64 = Base64UrlEncoder.Encode(jwtHeader.SerializeToJson());
            string jwtPayloadBase64 = Base64UrlEncoder.Encode(jwtPayload.SerializeToJson());

            var jwtSignRequest = new SignRequest
            {
                KeyId = Config.SignatureKey,
                //SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256, // Not supported by AWS Authorizer
                SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PKCS1_V1_5_SHA_256,
                MessageType = MessageType.RAW,
                Message = new MemoryStream(Encoding.UTF8.GetBytes($"{jwtHeaderBase64}.{jwtPayloadBase64}"))
            };

            SignResponse jwtSignResponse = await new AmazonKeyManagementServiceClient().SignAsync(jwtSignRequest);

            string jwtSignatureBase64 = Base64UrlEncoder.Encode(jwtSignResponse.Signature.ToArray());

            var tokenResponse = new TokenResponse
            {
                access_token = $"{jwtHeaderBase64}.{jwtPayloadBase64}.{jwtSignatureBase64}"
            };

            return new TokenSuccessResponse(200, tokenResponse);
        }

        public class TokenRequest
        {
            public string? grant_type { get; set; }
        }

        public class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; } = "urn:ietf:params:oauth:token-type:id_token";
            public int? expires_in { get; set; } = 0;
        }

        public class TokenErrorResponse : APIGatewayHttpApiV2ProxyResponse
        {
            public TokenErrorResponse(int statusCode, string message, string? errorDescription = null)
            {
                this.StatusCode = statusCode;
                var messsgeObject = new Dictionary<string, string>() { { "error", message } };
                if (errorDescription != null) { messsgeObject.Add("error_description", errorDescription); }
                Body = JsonSerializer.Serialize(messsgeObject);
            }
        }

        public class TokenSuccessResponse : APIGatewayHttpApiV2ProxyResponse
        {
            public TokenSuccessResponse(int statusCode, TokenResponse body)
            {
                this.StatusCode = statusCode;
                Body = JsonSerializer.Serialize(body);
            }
        }
    }
}
