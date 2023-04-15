using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;

namespace Technologai
{
    public class Identity
    {
        public string? Name { get; private set; }
        public string Id { get; private set; }        
        public string? AgencyId
        {
            get { return _agencyId ?? throw new ArgumentNullException(nameof(AgencyId)); }
            private set { _agencyId = value; }
        }
        internal Authority Authority { get; private set; }
        internal string ClientId { get; private set; }
        internal string ClientSecret { get; private set; }
        internal string? Token { get; private set; }
        internal string PublishMask => $"{AgencyId}/+";
        internal string SubscribeMemberMask => $"{AgencyId}/{Id}";
        internal string SubscribeAgencyMask => $"{AgencyId}/0";        

        private string? _agencyId;

        public Identity(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Authority = new Authority(authorityName);
            ClientId = clientId;
            ClientSecret = clientSecret;
            Id = memberId;
        }

        internal async Task Authenticate()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64UrlEncoder.Encode($"{ClientId}:{ClientSecret}"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "client_credentials");
                parameters.Add("scope", $"member:{Id}");

                var httpResponse = await httpClient.PostAsJsonAsync(Authority?.TokenEndpoint, parameters);

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var tokenResponse = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();

                    if (tokenResponse != null)
                    {
                        Token = tokenResponse.access_token;

                        foreach (Claim claim in new JwtSecurityTokenHandler().ReadJwtToken(Token).Claims)
                        {
                            if (claim.Type == "agency_id")
                            {
                                AgencyId = claim.Value;
                            }
                            if (claim.Type == "name")
                            {
                                Name = claim.Value;
                            }
                        }
                        return;
                    }
                }
                throw new HttpRequestException("Unauthorized", null, httpResponse.StatusCode);
            }
        }
        public string GetMaskedTopic(string topic)
        {
            string[] topicParts = topic.Split('/');
            string[] maskParts = PublishMask.Split('/');

            if (topicParts.Length != 2)
            {
                throw new ArgumentException(nameof(topic));
            }

            for (int i = 0; i < topicParts.Length; i++)
            {
                topicParts[i] = maskParts[i] == "+" ? topicParts[i] : maskParts[i];
            }
            return string.Join('/', topicParts);
        }

        internal class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
            public int? expires_in { get; set; }
        }
    }
}
