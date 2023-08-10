using System.Net.Http.Headers;
using System.Text;

namespace Technologai
{
    /*
    // Get Jira Tickets
    GetJiraTickets getJiraTickets = new();
    var getJiraTicketsDict = new Dictionary<string, object>()
             {
                 {"domain","your domain"},
                 {"issueID","your issueId" },
                 {"username","your username" },
                 {"password","your password (access_token)"}
             };
    var rr1 = getJiraTickets.Execute(getJiraTicketsDict).Result;
    */

    /// <summary>
    /// Get Jira Tickets
    /// </summary>
    public class GetJiraTickets : Template
    {
        public string Description { get; } = "Get Jira Tickets";
        public string SampleJsonIn { get; set; } = "{\"domain\":\"string\",\"username\":\"string\",\"password\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var domain = ((string)data["domain"]);
            var username = ((string)data["username"]);
            var password = ((string)data["password"]);

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/search")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                // Get comments
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var responseResult = await response.Content.ReadAsStringAsync();
                    return new Dictionary<string, object> { { "contents", responseResult } };
                }
                return new Dictionary<string, object> { { "Error", $"unable to find the response result" } };
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "Error", $"unable to find the comments  '{e.Message}'" } };
            }
        }
    }
}