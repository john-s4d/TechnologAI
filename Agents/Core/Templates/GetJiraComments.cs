using System.Net.Http.Headers;
using System.Text;

namespace Technologai
{
    /*
    // Get Jira Comments
    GetJiraComments getJiraComments = new();
    var getJiraCommentsDict = new Dictionary<string, object>()
             {
                 {"domain","your domain"},
                 {"issueID","your issueId" },
                 {"username","your username" },
                 {"password","your password (access_token)"}
             };
    var rr = getJiraComments.Execute(getJiraCommentsDict).Result;
    */

    /// <summary>
    /// GetJiraComments
    /// </summary>
    public class GetJiraComments : Template
    {
        public string Domain { get; set; }
        public string IssueID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }


        public string Description { get; } = "Get Jira Comments";
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
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}/comment")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

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