using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.CredentialManagement.Internal;
using System.Text.Json;
using Technologai.AWS.OpenID;

namespace Technologai.AWS
{
    internal class Icecast
    {   
        public APIGatewayHttpApiV2ProxyResponse ListenerAdd(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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

            // TODO: Check mount => claim.agency_id
            // member can listen to any stream in the agency.s
            // Check that mount is valid name

            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
        }

        public APIGatewayHttpApiV2ProxyResponse StreamAuth(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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

            // TODO: Check mount => claim.sub
            // member must post to their own mount            
            // ie: https://stream.technologai.com/abcdef0123456789
            // multiple mounts per member?

            return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = 200 };
        }

        public bool IsAuthorized(Dictionary<string, string> claims, out string message)
        {
            if (JsonSerializer.Deserialize<List<string>>(claims["aud"])?.Contains(Config.StreamUri) ?? false)
            {
                message = "wrong_audience";
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

            // TODO: Check that it is not expired
            message = string.Empty;
            return true;
        }

    }
}
