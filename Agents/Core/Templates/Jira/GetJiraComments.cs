using System.Net.Http.Headers;
using System.Text;
using Technologai;

namespace Core.Templates.Jira
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
        internal string Username { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        public GetJiraComments(string username, string password)
        {
            Id = "get_jira_comments";
            Description = "Get Jira Comments";
            InputKeys = new string[] { "domain", "issueID" };
            OutputKeys = new string[] { "content" };
            Username = username; 
            Password = password;   
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var domain = information.Input.Structured["domain"];
            var issueID = information.Input.Structured["issueID"];

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}/comment")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                // Get comments
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                return Data.Create("Error",  $"Unable to find the Jira ticket Status Code: '{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
        }
    }
}