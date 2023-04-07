using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace Technologai
{
    public class AgentIdentity
    {
        public string? Name { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TokenEndpoint { get; set; }
        internal string? Token { get; set; }

        internal async Task Authenticate()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64UrlEncoder.Encode($"{ClientId}:{ClientSecret}"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpResponse = await httpClient.PostAsJsonAsync(TokenEndpoint, new { grant_type = "client_credentials" });

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var tokenResponse = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();
                this.Token = tokenResponse?.access_token;
            }
            else
            {
                throw new HttpRequestException("Could not get Token", null, httpResponse.StatusCode);
            }
        }

        public class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; } 
            public int? expires_in { get; set; }
        }
    }
}
