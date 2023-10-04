using Newtonsoft.Json;
using System.IO;
using System.Text;
using Technologai;

namespace Core.Templates.Jira
{
    /*
    //Post Jira Comment
    PostJiraComment postJiraComment = new();
    var postJiraCommentDict = new Dictionary<string, object>()
              {
                      {"domain","your domain"},
                      {"issueID","your issueId" },
                      {"username","your username" },
                      {"password","your password (access_token)"},
                      {"textComment", "Hello Jira" }
              };
    var postJiraCommentResult = postJiraComment.Execute(postJiraCommentDict).Result;
    */

    /// <summary>
    /// Post Jira Comments
    /// </summary>
    public class PostJiraComment : Template
    {
        internal string Username { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        public PostJiraComment(string username, string password)
        {
            Id = "post_jira_comment";
            Description = "Post Jira Comments";
            InputKeys = new string[] { "domain", "issueID", "comment" };
            OutputKeys = new string[] { "content" };
            Username = username;
            Password = password;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var domain = information.Input.Structured["domain"];
            var issueID = information.Input.Structured["issueID"];
            var textComment = information.Input.Structured["textComment"];
            var configs = new
            {
                body = new
                {
                    content = new[] {
                        new { content = new [] {
                        new { text = textComment, type = "text"} }, type = "paragraph"}
                    },
                    type = "doc",
                    version = 1
                }
            };
            var jsonData = JsonConvert.SerializeObject(configs);
            var byteArray = Encoding.ASCII.GetBytes($"{Username} : {Password}");

            try
            {
                var url = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}/comment");
                // Post comments
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                using var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                return Data.Create("Error", $"Unable to Post Jira Comments Status Code: '{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
        }
    }
}