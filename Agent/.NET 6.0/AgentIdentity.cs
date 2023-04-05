using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Technologai
{
    public class AgentIdentity
    {   

        public string? TokenEndpoint { get; set; }
        public string? ApiKey { get; set; }
        internal string? Token { get; set; } 

        internal async Task Authenticate()
        {   
            var tokenUrl = this.TokenEndpoint ?? throw new ArgumentNullException(nameof(this.TokenEndpoint));

            var tokenRequest = new TokenRequest
            {
                ApiKey = this.ApiKey ?? throw new ArgumentNullException(nameof(this.ApiKey))
            };

            var httpResponse = await new HttpClient().PostAsJsonAsync(tokenUrl, tokenRequest);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var tokenResponse = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();
                this.Token = tokenResponse?.Token;
            }
            else
            {
                throw new HttpRequestException("Could not get Token", null, httpResponse.StatusCode);
            }
        }

        private class TokenRequest
        {
            public string? ApiKey { get; set; }
        }

        public class TokenResponse
        {
            public string? Token { get; set; }
        }
    }
}
