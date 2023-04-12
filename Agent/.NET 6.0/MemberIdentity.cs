using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Technologai
{
    public class MemberIdentity : Identity
    {
        //internal override string RoleName => "member";
        internal override TechnologaiRole RoleName => TechnologaiRole.member;
        public AgentIdentity Agent { get; }
        public AgencyIdentity? Agency { get; internal set; }
        public Authority Authority => Agent.Authority;
        internal string? Token { get; private set; }
        internal override string PublishMask => $"{Agency?.Id ?? "0"}/0/0/0";
        internal override string SubscribeMask => $"{Agency?.Id ?? "0"}/{Id}/0/0";

        internal MemberIdentity(string id, AgentIdentity agent)
        {
            Id = id;
            Agent = agent;
        }

        internal async Task Authenticate()
        {

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Agent.Bearer);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "client_credentials");
                parameters.Add("scope", $"{RoleName}:{Id}");

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
                                Agency = new AgencyIdentity(claim.Value, Agent);
                            }
                            if (claim.Type == "name")
                            {
                                Name = claim.Value;
                            }
                            if (claim.Type == "role")
                            {
                                AssignedRole = Enum.Parse<TechnologaiRole>(claim.Value);
                            }
                        }
                        return;
                    }
                }
                throw new HttpRequestException("Unauthorized", null, httpResponse.StatusCode);
            }
        }

        internal class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
            public int? expires_in { get; set; }
        }
    }
}
