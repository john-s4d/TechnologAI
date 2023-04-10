using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.CredentialManagement.Internal;
using Amazon.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Technologai.AWS.OpenID;
using static Technologai.AWS.OpenID.QueryAdapter;

namespace Technologai.AWS
{
    internal class Mqtt
    {
        public APIGatewayHttpApiV2ProxyResponse GetUser(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            if (request.RequestContext.Authorizer == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "Unauthorized" };
            }

            Dictionary<string, string> claims = new Dictionary<string, string>(request.RequestContext.Authorizer.Jwt.Claims);

            if (!IsAuthorized(claims, out string message))
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = message };
            }

            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
        }

        public APIGatewayHttpApiV2ProxyResponse AclCheck(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            // Validating JWT General
            if (request.RequestContext.Authorizer == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "Unauthorized" };
            }

            Dictionary<string, string> claims = new Dictionary<string, string>(request.RequestContext.Authorizer.Jwt.Claims);

            if (!IsAuthorized(claims, out string message))
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = message };
            }

            // STRUCTURE: agency/member/agent/subagent

            // acc: - 1 is read, 2 is write, 3 is readwrite, 4 is subscribe

            AclCheckRequest? acl = JsonSerializer.Deserialize<AclCheckRequest>(request.Body);

            if (acl == null || acl.topic == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "missing_topics" };
            }

            if (acl.acc == 3)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "readwrite_not_allowed" };
            }

            string agentId = claims["client_id"];
            string memberId = claims["sub"];
            string agencyId = claims["agency_id"];
            string role = claims["role"];

            if (role == "agent")
            {
                string publishMask = $"0/0/{agentId}/+";
                string subscribeMask = $"0/0/{agentId}/+";

                if ((acl.acc == 1 || acl.acc == 4) && TopicAllowed(acl.topic, subscribeMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }
                if (acl.acc == 2 && !acl.topic.Contains('+') && TopicAllowed(acl.topic, publishMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }
            }
            if (role == "member")
            {
                string publishMask = $"{agencyId}/0/0/0";
                string subscribeMask = $"{agencyId}/{memberId}/0/0";

                if ((acl.acc == 1 || acl.acc == 4) && TopicAllowed(acl.topic, subscribeMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }
                if (acl.acc == 2 && !acl.topic.Contains('+') && TopicAllowed(acl.topic, publishMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }
            }
            if (role == "agency")
            {
                string publishMask = $"{agencyId}/+/0/0";
                string subscribeMask = $"{agencyId}/0/0/0";

                if ((acl.acc == 1 || acl.acc == 4) && TopicAllowed(acl.topic, subscribeMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }
                if (acl.acc == 2 && !acl.topic.Contains('+') && TopicAllowed(acl.topic, publishMask))
                {
                    return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
                }

            }

            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401 };
        }

        private bool TopicAllowed(string? topic, string mask)
        {
            if (topic == null)
            {
                return false;
            }

            string[] topicParts = topic.Split('/');
            string[] maskParts = mask.Split('/');

            if (topicParts.Length != maskParts.Length)
            {
                return false;
            }

            for (int i = 0; i < topicParts.Length; i++)
            {
                if (maskParts[i] != "+" && maskParts[i] != topicParts[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAuthorized(Dictionary<string, string> claims, out string message)
        {
            if (!claims.ContainsKey("aud") || string.IsNullOrEmpty(claims["aud"]))
            {
                message = "audience_missing";
                return false;
            }

            if (claims["aud"] != Config.TokenAudience)
            {
                message = "wrong_audience";
                return false;
            }

            if (!claims.ContainsKey("role") || string.IsNullOrEmpty(claims["role"]))
            {
                message = "role_missing";
                return false;
            }

            if (!claims.ContainsKey("client_id") || string.IsNullOrEmpty(claims["client_id"]))
            {
                message = "client_id_missing";
                return false;
            }

            if (!claims.ContainsKey("agency_id") || string.IsNullOrEmpty(claims["agency_id"]))
            {
                message = "agency_id_missing";
                return false;
            }

            if (!claims.ContainsKey("sub") || string.IsNullOrEmpty(claims["sub"]))
            {
                message = "sub_missing";
                return false;
            }

            if (!claims.ContainsKey("name") || string.IsNullOrEmpty(claims["name"]))
            {
                message = "name_missing";
                return false;
            }
            message = string.Empty;
            return true;
        }

        public class AclCheckRequest
        {
            public int? acc { get; set; }
            public string? clientid { get; set; }
            public string? topic { get; set; }
        }

        public class GetUserRequest
        {
            public string? grant_type { get; set; }
        }
    }
}
