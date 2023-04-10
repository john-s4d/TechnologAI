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
using System.Runtime.CompilerServices;
using static System.Formats.Asn1.AsnWriter;
using Amazon.Runtime.Internal.Transform;

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
                    string query = string.Empty;

                    foreach (string scope in new List<string>(tokenRequest.scope?.Split(' ') ?? new string[] { }))
                    {
                        if (scope == "agent")
                        {
                            query = $"SELECT Name FROM Agent__c WHERE Agent_Id__c = {clientId} LIMIT 1";
                            throw new NotImplementedException(); // TODO: agents should be able to connect without a member
                        }
                        else if (scope.StartsWith("member:") || scope.StartsWith("agency:"))
                        {
                            var memberId = scope.Substring(scope.IndexOf(':') + 1).Replace("'", string.Empty); ; // Light sanitizing since this could have been constructed manually

                            // TODO: Do Salesforce stuff somewhere else
                            query = $"SELECT Member_Id__c, Name, Agency__r.Name, Agency__r.Agency_Id__c, Agent__r.Name, Agent__r.Agent_Id__c, Role__c " +
                                    $"FROM Agency_Member__c WHERE Member_Id__c = '{memberId}' AND Agent__r.Agent_Id__c = '{clientId}' LIMIT 1";
                            break;
                        }
                    }

                    TokenClaims? tokenClaims = await QueryAdapter.GetClaimsFromSalesforce(query);

                    if (tokenClaims != null)
                    {
                        var claims = new Dictionary<string, string>();
                        claims.Add("sub", tokenClaims.member_id ?? string.Empty);
                        claims.Add("name", tokenClaims.name ?? string.Empty);
                        claims.Add("role", tokenClaims.role ?? string.Empty);
                        claims.Add("client_id", tokenClaims.client_id ?? string.Empty);
                        claims.Add("agency_id", tokenClaims.agency_id ?? string.Empty);
                        claims.Add("aud", Config.TokenAudience);

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

            //jwtPayload.Add("scp", "");

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
            public string? scope { get; set; }
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
        public string? client_id { get; set; }
        public string? member_id { get; set; }
        public string? agency_id { get; set; }
        public string? role { get; set; }
    }
}
