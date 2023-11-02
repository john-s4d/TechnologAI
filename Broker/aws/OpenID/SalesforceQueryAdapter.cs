using System.Net.Http.Headers;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Technologai.AWS.OpenID
{
    internal class SalesforceQueryAdapter
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static string? _sfdcEndpointHost;        
        private static DateTime? _salesforceSessionCreated;
        private static string? _salesforceSessionId;

        static SalesforceQueryAdapter()
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal static async Task Authenticate()
        {
            if (string.IsNullOrEmpty(_salesforceSessionId) ||
                    _salesforceSessionCreated == null ||
                    _salesforceSessionCreated?.AddMinutes(Config.SfdcSessionExpiryMinutes) > DateTime.UtcNow
               )
            {
                GetSecretValueRequest secretRequest = new GetSecretValueRequest
                {
                    SecretId = Config.SalesforceApiSecretArn
                };

                GetSecretValueResponse secretResponse = await new AmazonSecretsManagerClient().GetSecretValueAsync(secretRequest);

                var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(secretResponse.SecretString) ?? new();

                _sfdcEndpointHost = secrets["SalesforceEndpointHost"];
                var sfdcClientId = secrets["SalesforceApiClientId"];
                var sfdcClientSecret = secrets["SalesforceApiClientSecret"];

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://{_sfdcEndpointHost}/services/oauth2/token");

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Base64UrlEncoder.Encode($"{sfdcClientId}:{sfdcClientSecret}"));
                requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

                var authResponse = await httpClient.SendAsync(requestMessage);

                if (authResponse.IsSuccessStatusCode)
                {
                    _salesforceSessionId = JsonSerializer.Deserialize<Dictionary<string, string>>(await authResponse.Content.ReadAsStringAsync())?["access_token"];
                    _salesforceSessionCreated = DateTime.UtcNow;
                }
                else
                {
                    throw new HttpRequestException($"Salesforce Authentication Error {authResponse.StatusCode}");
                }                
            }
            if (_salesforceSessionId == null) { throw new HttpRequestException($"Could not authenticate."); }
        }

        internal static async Task<SalesforceQueryResponse<T>> Query<T>(string query) where T : sObject
        {
            await Authenticate();

            if (string.IsNullOrEmpty(_sfdcEndpointHost)) { throw new ArgumentNullException(nameof(_sfdcEndpointHost)); }

            var requestMessage = new HttpRequestMessage();
            requestMessage.Method = HttpMethod.Get;
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _salesforceSessionId);
            requestMessage.RequestUri = new Uri($"https://{_sfdcEndpointHost}/services/data/v56.0/query?q={Uri.EscapeDataString(query)}");

            var response = await httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SalesforceQueryResponse<T>>(resultString) ?? new();
            }
            else
            {
                throw new HttpRequestException($"Salesforce Query Error {response.StatusCode}");
            }
        }

        internal static async Task<TokenClaims?> GetClaims(string instanceId, string agentId)
        {   
            var query = $"SELECT Name, Agent_Id__c, Agency__r.Agency_Id__c, Instance__r.Instance_Id__c " +
                        $"FROM Agent__c WHERE Agent_Id__c = '{agentId}' AND Instance__r.Instance_Id__c = '{instanceId}' LIMIT 1";

            var result = await Query<Agent>(query);

            if (result?.totalSize != null && result?.totalSize == 1)
            {
                Agent record = result.records[0];

                TokenClaims tokenClaims = new TokenClaims();
                tokenClaims.name = record?.Name;
                tokenClaims.agent_id = record?.Agent_Id__c;
                tokenClaims.agency_id = record?.Agency__r.Agency_Id__c;
                tokenClaims.instance_id = record?.Instance__r.Instance_Id__c;

                // Invalidate ones that are incomplete
                if (!string.IsNullOrEmpty(tokenClaims.name) &&
                    !string.IsNullOrEmpty(tokenClaims.agent_id) &&
                    !string.IsNullOrEmpty(tokenClaims.agency_id) &&
                    !string.IsNullOrEmpty(tokenClaims.instance_id)                    
                    )
                {
                    return tokenClaims;
                }
            }
            return null;
        }

        public class sObject { }

        public class Agent : sObject
        {
            public string Name { get; set; }
            public string Agent_Id__c { get; set; }
            public Agency Agency__r { get; set; }            
            public Instance Instance__r { get; set; }            
        }

        public class Agency : sObject
        {
            public string Agency_Id__c { get; set; }
        }

        public class Instance : sObject
        {            
            public string Instance_Id__c { get; set; }
        }

        public class SalesforceQueryResponse<T> where T : sObject
        {
            public int totalSize { get; set; } = 0;
            public bool done { get; set; } = true;
            public List<T> records { get; set; } = new();
        }
    }
}
