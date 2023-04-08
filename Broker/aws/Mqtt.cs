using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Technologai.AWS
{
    internal class Mqtt
    {
        public APIGatewayHttpApiV2ProxyResponse GetUser(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            // Ensure the Authorizer saw this request
            if (request.RequestContext.Authorizer == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "Unauthorized" };
            }
            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> AclCheck(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"request: {JsonSerializer.Serialize(request)}");
            LambdaLogger.Log($"context: {JsonSerializer.Serialize(context)}");

            if (request.RequestContext.Authorizer == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 401, Body = "Unauthorized" };
            }

            // TODO: System State Topic
            // TODO: Validate Timestamp in Topic

            // STRUCTURE: agency/member/context/agent/subagent

            // Private Agent Topic
            // subscribe: 0/0/0/<agent>/+
            // publish: 0/0/0/<agent>/+

            // Agent Member Topics
            // subscribe: <agency>/<member>/+/0/0
            // publish: <agency>/0/<context>/0/0

            // Agency (Coordinator,Archiver,Regulator, etc..) Topics
            // subscribe: <agency>/0/+/0/0
            // publish: <agency>/<member>/<context>/0/0


            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
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
