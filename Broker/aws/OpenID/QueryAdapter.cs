using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using Amazon.Runtime.Internal.Transform;

namespace Technologai.AWS.OpenID
{
    internal class QueryAdapter
    {

        private static string? _salesforceSessionId;
        private static DateTime? _salesforceSessionCreated;

        internal static async Task<TokenClaims?> GetClaimsFromSalesforce(string query)
        {
            GetSecretValueRequest secretRequest = new GetSecretValueRequest
            {
                SecretId = Config.SalesforceApiSecretArn
            };

            GetSecretValueResponse secretResponse = await new AmazonSecretsManagerClient().GetSecretValueAsync(secretRequest);

            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(secretResponse.SecretString);

            string sfdcClientId = secrets["SalesforceApiClientId"];
            string sfdcClientSecret = secrets["SalesforceApiClientSecret"];
            string sfdcEndpointHost = secrets["SalesforceEndpointHost"];

            using (var httpClient = new HttpClient())
            {

                // AUTHENTICATE
                // TODO: Is this properly cached?
                if (string.IsNullOrEmpty(_salesforceSessionId) &&
                        (_salesforceSessionCreated == null || DateTime.UtcNow.AddMinutes(Config.SfdcSessionExpiryMinutes * -1) > _salesforceSessionCreated)
                   )
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64UrlEncoder.Encode($"{sfdcClientId}:{sfdcClientSecret}"));

                    var endpoint = $"https://{Uri.EscapeDataString(sfdcEndpointHost)}/services/oauth2/token";

                    var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

                    var authResponse = await httpClient.PostAsync(endpoint, content);

                    if (authResponse.IsSuccessStatusCode)
                    {
                        _salesforceSessionId = JsonSerializer.Deserialize<Dictionary<string, string>>(await authResponse.Content.ReadAsStringAsync())?["access_token"];
                        _salesforceSessionCreated = DateTime.UtcNow;
                    }
                }

                if (!string.IsNullOrEmpty(_salesforceSessionId))
                {
                    
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _salesforceSessionId);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var url = $"https://{Uri.EscapeDataString(sfdcEndpointHost)}/services/data/v56.0/query?q={Uri.EscapeDataString(query)}";

                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var salesforceResultString = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<SalesforceQueryResponse>(salesforceResultString);

                        if (result?.totalSize != null && result?.totalSize == 1)
                        {
                            TokenClaims tokenClaims = new TokenClaims();

                            AgencyMember? record = result.records[0];

                            tokenClaims.name = record?.Name;
                            tokenClaims.client_id = record?.Agent__r.Agent_ID__c;

                            tokenClaims.roles = new List<string>();

                            if (!string.IsNullOrEmpty((record?.Roles__c as string)))
                            {
                                tokenClaims.roles.AddRange(record.Roles__c.Split(';'));
                            }

                            if (tokenClaims.roles.Contains("member"))
                            {
                                tokenClaims.member_id = record?.Member_Id__c;
                                tokenClaims.agency_id = record?.Agency__r.Agency_Id__c;                                
                            }
                            return tokenClaims;
                        }
                    }
                }
            }
            // TODO: Error Handling
            return null;
        }
        public class AgencyMember
        {
            public Attributes attributes { get; set; }
            public string Member_Id__c { get; set; }
            public string Name { get; set; }
            public Agency Agency__r { get; set; }
            public Agent Agent__r { get; set; }
            public string Roles__c { get; set; }
        }

        public class Attributes
        {
            public string type { get; set; }
            public string url { get; set; }
        }

        public class Agency
        {
            public Attributes attributes { get; set; }
            public string Name { get; set; }
            public string Agency_Id__c { get; set; }
        }

        public class Agent
        {
            public Attributes attributes { get; set; }
            public string Name { get; set; }
            public string Agent_ID__c { get; set; }
        }

        public class SalesforceQueryResponse
        {
            public int totalSize { get; set; }
            public bool done { get; set; }
            public List<AgencyMember> records { get; set; }
        }
    }
}
