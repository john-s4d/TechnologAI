using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.AWS
{
    internal class Mqtt
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> GetUser(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            // Check that JWT is verified by Authorizer
            // Ask Salesforce if the clientId is valid
            return new APIGatewayHttpApiV2ProxyResponse();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> AclCheck(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            // Check that JWT is verified by Authorizer
            // Always allow Agent & State Channels
            // Ask Salesforce if the Acl Route is allowed.
            return new APIGatewayHttpApiV2ProxyResponse();
        }
    }
}
