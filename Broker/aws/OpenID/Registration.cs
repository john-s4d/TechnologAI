using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Technologai.AWS.OpenID
{
    internal class Registration
    {
        // TODO: Provide method to deactivate ClientId

        public async Task<APIGatewayHttpApiV2ProxyResponse> ClientPost(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            if (request.RequestContext.Authorizer == null)
            {
                LambdaLogger.Log("No Authorizer");
                return new ClientErrorResponse(401, "Unauthorized");
            }
            else
            {
                // TODO: Do we need to validate Authorizer subject? Ideally issuer trust should be enough.
            }

            byte[] clientIdBytes = Array.Empty<byte>();
            JsonWebKey jsonWebKey = new();

            try
            {
                // TODO: Better null error handling, error description to caller

                var clientRequest = JsonSerializer.Deserialize<ClientMetaData>(request.Body);

                clientIdBytes = Base64UrlEncoder.DecodeBytes(clientRequest?.preferred_client_id);
                jsonWebKey = new JsonWebKey(clientRequest?.json_web_key);

                if (clientIdBytes.Length != 32) { throw new ArgumentException(nameof(ClientMetaData.preferred_client_id)); }
            }
            catch (Exception ex)
            {
#if DEBUG
                if (request.Body == "DEBUG")
                {
                    clientIdBytes = RandomNumberGenerator.GetBytes(32);
                    jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(RSA.Create(2048).ExportParameters(false)));
                    
                    var clientRequest = new ClientMetaData
                    {
                        preferred_client_id = Base64UrlEncoder.Encode(clientIdBytes),
                        json_web_key = JsonExtensions.SerializeToJson(jsonWebKey)
                    };
                    LambdaLogger.Log(JsonSerializer.Serialize(clientRequest));
                }
#else
                LambdaLogger.Log(ex.Message);
                return new ClientErrorResponse(400, "invalid_request");
#endif
            }

            byte[] clientSecret = RandomNumberGenerator.GetBytes(32);
            byte[] salt = RandomNumberGenerator.GetBytes(32);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            string clientId = Base64UrlEncoder.Encode(clientIdBytes);

            DateTime clientIssuedAt = DateTime.UtcNow;

            var putRequest = new PutItemRequest
            {
                TableName = Config.CLIENT_TABLE_NAME,
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "ClientSecretSaltHash", new AttributeValue { S = Base64UrlEncoder.Encode(clientSecretSaltHash) } },
                        { "Salt", new AttributeValue { S = Base64UrlEncoder.Encode(salt) } },
                        { "ClientId", new AttributeValue { S = clientId } },
                        { "CreatedDateTime", new AttributeValue { S = clientIssuedAt.ToString("o") } },
                        { "CreatedBy", new AttributeValue { S = request.RequestContext.Authorizer != null ? request.RequestContext.Authorizer.Jwt.Claims["sub"] : string.Empty } },
                        { "Active", new AttributeValue { BOOL = true } }
                    },
                ConditionExpression = "attribute_not_exists(ClientId)"

            };

            try
            {
                await new AmazonDynamoDBClient().PutItemAsync(putRequest);
            }
            catch (ConditionalCheckFailedException)
            {
                return new ClientErrorResponse(409, "preferred_client_id_already_exists");
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.Message);
                return new ClientErrorResponse(500, "internal_server_error");
            }

#if DEBUG   
            LambdaLogger.Log($"client_id: {clientId}");
            LambdaLogger.Log($"client_secret: {Base64UrlEncoder.Encode(clientSecret)}");
#endif

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(JwkToRsa(jsonWebKey));

                var clientInformation = new ClientInformation
                {
                    client_id = clientId,
                    encrypted_client_secret = Base64UrlEncoder.Encode(rsa.Encrypt(clientSecret, false)),
                    client_id_issued_at = Convert.ToString(new DateTimeOffset(clientIssuedAt).ToUnixTimeMilliseconds()),
                    //registration_access_token = "",
                    registration_client_uri = Config.RegistrationEndpoint
                };
                return new ClientSuccessResponse(200, clientInformation);
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

        public class ClientMetaData
        {
            public string? preferred_client_id { get; set; }
            public string? json_web_key { get; set; }
        }

        public class ClientInformation
        {
            public string? client_id { get; set; }
            public string? client_secret { get; set; }
            public string? client_id_issued_at { get; set; }
            public string? encrypted_client_secret { get; set; }
            public string? registration_access_token { get; set; }
            public string? registration_client_uri { get; set; }
        }

        public class ClientSuccessResponse : APIGatewayHttpApiV2ProxyResponse
        {
            public ClientSuccessResponse(int statusCode, ClientInformation body)
            {
                this.StatusCode = statusCode;                
                Body = JsonSerializer.Serialize(body);
            }
        }

        public class ClientErrorResponse : APIGatewayHttpApiV2ProxyResponse
        {
            public ClientErrorResponse(int statusCode, string message, string? errorDescription = null)
            {
                this.StatusCode = statusCode;
                var messsgeObject = new Dictionary<string, string>() { { "error", message } };
                if (errorDescription != null) { messsgeObject.Add("error_description", errorDescription); }
                Body = JsonSerializer.Serialize(messsgeObject);
            }
        }
    }
}
