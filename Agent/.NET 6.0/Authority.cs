using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace Technologai
{
    public class Authority
    {
        public string Host { get; private set; } = "auth.technologai.com";
        public string? ClientEndpoint { get; private set; }
        public string? TokenEndpoint { get; private set; }
        public string BrokerHost { get; private set; }
        public Authority(string host)
        {
            // TODO: Connect to Discovery Endpoint and get the correct values

            if (host != "auth.technologai.com") { throw new NotImplementedException(); }

            this.Host = host;
            this.BrokerHost = "broker.technologai.com";
            this.ClientEndpoint = "https://auth.technologai.com/client";
            this.TokenEndpoint = "https://auth.technologai.com/token";
        }

        internal async Task Authenticate(Identity identity)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.Bearer);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpResponse = await httpClient.PostAsJsonAsync(TokenEndpoint, new { grant_type = identity.GrantType});

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var tokenResponse = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();

                if (tokenResponse != null)
                {
                    identity.Token = tokenResponse.access_token;
                    return;
                }
            }
            throw new HttpRequestException("Unauthorized", null, httpResponse.StatusCode);
        }

        internal class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
            public int? expires_in { get; set; }
        }
    }
}
