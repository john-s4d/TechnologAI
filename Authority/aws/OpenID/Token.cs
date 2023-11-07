using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.KeyManagementService.Model;
using Amazon.KeyManagementService;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using Amazon.Util;

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
                    return new TokenErrorResponse(415, "unsupported_media_type");
                }

                var tokenRequest = JsonSerializer.Deserialize<TokenRequest>(request.Body) ?? throw new ArgumentNullException(nameof(request.Body));

                string authHeader = headers[HeaderKeys.AuthorizationHeader] ?? throw new ArgumentNullException(nameof(request.Body));

                if (tokenRequest.grant_type != "client_credentials")
                {
                    return new TokenErrorResponse(400, "unsupported_grant_type");
                }

                string[] credentials = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(authHeader.Substring("Basic ".Length).Trim())).Split(':');
                clientId = credentials[0];
                clientSecret = credentials[1];

                var dynamoDbClient = new AmazonDynamoDBClient();

                var querySaltRequest = new QueryRequest
                {
                    TableName = Config.AWSClientTableName,
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

                if (inputClientSecretSaltHash == dbClientSecretSaltHash)
                {

                    // Crypto authentication done. Now validate credentials.                   

                    var scope = tokenRequest.scope.Split(' ', StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);

                    var agentId = GetValueForKey(scope, "agent_id") ?? throw new Exception("agent_id not found in scope");

                    var tokenClaims = await SalesforceQueryAdapter.GetClaims(clientId, agentId);

                    if (tokenClaims != null)
                    {
                        var claims = new Dictionary<string, string>();
                        claims.Add("sub", tokenClaims.agent_id ?? string.Empty);
                        claims.Add("name", tokenClaims.name ?? string.Empty);

                        // TODO: collision resistant claim names
                        claims.Add("agency_id", tokenClaims.agency_id ?? string.Empty);
                        claims.Add("instance_id", tokenClaims.instance_id ?? string.Empty);
                        claims.Add("role", "agent"); // client_credentials grants are only for agents.

                        if (tokenRequest.audience?.Equals(Config.BrokerUri) ?? false)
                        {
                            claims.Add("aud", Config.BrokerUri);
                        }

                        if (tokenRequest.audience?.Equals(Config.StreamUri) ?? false)
                        {
                            claims.Add("aud", Config.StreamUri);
                        }

                        if (claims["aud"] == null)
                        {
                            throw new Exception("Invalid audience");
                        }

                        claims.Add("scp", tokenRequest.scope ?? string.Empty);

                        return new TokenSuccessResponse(200, new() { access_token = await getIdToken(claims) });
                    }

                    /* TODO:
                    The authorization server MUST include the HTTP "Cache-Control"
                    response header field [RFC2616] with a value of "no-store" in any
                    response containing tokens, credentials, or other sensitive
                    information, as well as the "Pragma" response header field [RFC2616]
                    with a value of "no-cache".
                    */
                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.Message);
                return new TokenErrorResponse(400, "Bad Request");
            }

            return new TokenErrorResponse(401, "Unauthorized");
        }

        static string? GetValueForKey(string[] keyValuePairs, string key, char separator = ':')
        {
            foreach (string pair in keyValuePairs)
            {
                if (pair.StartsWith(key + separator))
                {
                    return pair.Substring(pair.IndexOf(separator) + 1).Trim();
                }
            }
            return null;
        }

        private async Task<string> getIdToken(Dictionary<string, string> claims)
        {
            var kms = new AmazonKeyManagementServiceClient();

            var jwtHeader = new JwtHeader();
            jwtHeader.Add("alg", "RS256");
            jwtHeader.Add("typ", "JWT");
            jwtHeader.Add("kid", Config.SignatureKey);

            var jwtPayload = new JwtPayload();
            jwtPayload.Add("exp", Convert.ToString(DateTimeOffset.UtcNow.AddSeconds(Config.JwtExpirySeconds).ToUnixTimeSeconds()));
            jwtPayload.Add("iat", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            jwtPayload.Add("nbf", Convert.ToString(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            jwtPayload.Add("iss", Config.Issuer);

            foreach (string key in claims.Keys)
            {
                jwtPayload.Add(key, claims[key]);
            }

            string jwtHeaderBase64 = Base64UrlEncoder.Encode(jwtHeader.SerializeToJson());
            string jwtPayloadBase64 = Base64UrlEncoder.Encode(jwtPayload.SerializeToJson());

            var jwtSignRequest = new SignRequest
            {
                KeyId = Config.SignatureKey,
                SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PKCS1_V1_5_SHA_256,
                MessageType = MessageType.RAW,
                Message = new MemoryStream(Encoding.UTF8.GetBytes($"{jwtHeaderBase64}.{jwtPayloadBase64}"))
            };

            SignResponse jwtSignResponse = await new AmazonKeyManagementServiceClient().SignAsync(jwtSignRequest);

            string jwtSignatureBase64 = Base64UrlEncoder.Encode(jwtSignResponse.Signature.ToArray());

            return $"{jwtHeaderBase64}.{jwtPayloadBase64}.{jwtSignatureBase64}";
        }

        public class TokenRequest
        {
            public string? grant_type { get; set; }
            public string scope { get; set; } = string.Empty;
            public string audience { get; set; } = string.Empty;
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
    public class TokenClaims
    {
        public string? name { get; set; }
        public string? agent_id { get; set; }
        public string? agency_id { get; set; }
        public string? instance_id { get; set; }
    }
}
