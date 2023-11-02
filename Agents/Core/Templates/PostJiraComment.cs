using Newtonsoft.Json;
using System.Text;

namespace Technologai.Templates
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
        public string Description { get; } = "Post Jira Comments";
        public string SampleJsonIn { get; set; } = "{\"domain\":\"string\",\"issueID\":\"string\",\"username\":\"string\",\"password\":\"string\",\"comment\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var domain = ((string)data["domain"]);
            var issueID = ((string)data["issueID"]);
            var username = ((string)data["username"]);
            var password = ((string)data["password"]);
            var textComment = ((string)data["textComment"]);
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
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");

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
                    var responseResult = await response.Content.ReadAsStringAsync();
                    return new Dictionary<string, object> { { "contents", responseResult } };
                }
                return new Dictionary<string, object> { { "Error", $"unable to find the responseresult" } }; ;
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "Error", $"unable to find the comments  '{e.Message}'" } };
            }
        }
    }
}