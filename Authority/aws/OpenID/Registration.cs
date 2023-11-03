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
        // TODO: Harden. Use a real OAuth2 library. Probably this one: https://github.com/DuendeSoftware/IdentityServer

        public async Task<APIGatewayHttpApiV2ProxyResponse> ClientPost(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            if (request.RequestContext.Authorizer == null)
            {
                LambdaLogger.Log("No Authorizer");
                return new ClientErrorResponse(401, "Unauthorized");
            }           

            string? preferredClientId = string.Empty;
            JsonWebKey jwks = new();

            try
            {
                var clientRequest = JsonSerializer.Deserialize<ClientMetaData>(request.Body);

                preferredClientId = clientRequest?.preferred_client_id;
                jwks = new JsonWebKey(clientRequest?.jwks);

                if (preferredClientId?.Length != 32) { throw new ArgumentException(nameof(ClientMetaData.preferred_client_id)); }
            }
            catch (Exception ex)
            {
#if DEBUG

                // When in debug mode, we can generate a client key.
                // Update claim "sub" in the request to something helpful.

                if (request.Body == "GENERATE_CLIENT_REQUEST")
                {
                    preferredClientId = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
                    jwks = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(RSA.Create(2048).ExportParameters(false)));
                    
                    var clientMetaData = new ClientMetaData
                    {
                        preferred_client_id = preferredClientId,
                        jwks = JsonExtensions.SerializeToJson(jwks)
                    };
                    LambdaLogger.Log(JsonSerializer.Serialize(clientMetaData));
                }
#else
                LambdaLogger.Log(ex.Message);
                return new ClientErrorResponse(400, "invalid_request");
#endif
            }

            byte[] clientSecret = RandomNumberGenerator.GetBytes(32);
            byte[] salt = RandomNumberGenerator.GetBytes(32);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(clientSecret.Concat(salt).ToArray());

            //string clientId = Base64UrlEncoder.Encode(preferredClientId);

            DateTime clientIssuedAt = DateTime.UtcNow;

            var putRequest = new PutItemRequest
            {
                TableName = Config.AWSClientTableName,
                Item = new Dictionary<string, AttributeValue>
                    {
                        { "ClientSecretSaltHash", new AttributeValue { S = Base64UrlEncoder.Encode(clientSecretSaltHash) } },
                        { "Salt", new AttributeValue { S = Base64UrlEncoder.Encode(salt) } },
                        { "ClientId", new AttributeValue { S = preferredClientId } },
                        { "CreatedDateTime", new AttributeValue { S = clientIssuedAt.ToString("o") } },
                        { "CreatedBy", new AttributeValue { S = request.RequestContext.Authorizer.Jwt.Claims["sub"] } },
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
            LambdaLogger.Log($"client_id: {preferredClientId}");
            LambdaLogger.Log($"client_secret: {Base64UrlEncoder.Encode(clientSecret)}");
#endif

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(JwkToRsa(jwks));

                var clientInformation = new ClientInformation
                {
                    client_id = preferredClientId,
                    encrypted_client_secret = Base64UrlEncoder.Encode(rsa.Encrypt(clientSecret, false)),
                    client_id_issued_at = Convert.ToString(new DateTimeOffset(clientIssuedAt).ToUnixTimeMilliseconds()),                    
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
            public string? jwks { get; set; }
        }

        public class ClientInformation
        {
            public string? client_id { get; set; }
            public string? client_id_issued_at { get; set; }
            public string? encrypted_client_secret { get; set; } // TODO: NOT defined in RFC. Requires more protocol stuff. See https://tools.ietf.org/html/rfc7591#section-3.2.1
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
