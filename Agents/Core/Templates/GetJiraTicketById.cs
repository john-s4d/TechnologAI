using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Technologai.Agents.Core.DataModels;

namespace Technologai.Templates
{
    /*
    // Get Jira Ticket By Id
    GetJiraTicketById getJiraTicketById = new();
    var getJiraTicketByIdDict = new Dictionary<string, object>()
             {
                 {"domain","your domain"},
                 {"issueID","your issueId" },
                 {"username","your username" },
                 {"password","your password (access_token)"}
             };
    var rr1 = getJiraTicketById.Execute(getJiraTicketByIdDict).Result;
    */

    /// <summary>
    /// Get Jira Ticket by Id
    /// </summary>
    public class GetJiraTicketById : Template
    {
        public string Description { get; } = "Get Jira Ticket By Id";
        public string SampleJsonIn { get; set; } = "{\"domain\":\"string\",\"issueID\":\"string\",\"username\":\"string\",\"password\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var domain = ((string)data["domain"]);
            var issueID = ((string)data["issueID"]);
            var username = ((string)data["username"]);
            var password = ((string)data["password"]);

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}")
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
                    var deserializedData = JsonConvert.DeserializeObject<Root>(responseResult);
                    return new Dictionary<string, object> { { "contents", deserializedData } };
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