using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Technologai.Templates
{
    /// <summary>
    /// Post To Reddit
    /// </summary>
    public class PostToReddit : Template
    {

        /*
        //PostToReddit
        PostToReddit postToReddit = new();
        var postToRedditDict = new Dictionary<string, object>()
            {
                {"clientId","clientID"},
                {"clientSecret","clientSecret"},
                {"username","username"},
                {"password","password"},
                {"subreddit","subreddit name"},
                {"title","title"},
                {"text","your text" },
                {"redditAppName","reddit app name"}
            };
        var postToRedditResponse = postToReddit.Execute(postToRedditDict).Result;
        */

        internal string Username { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        public PostToReddit(string username, string password)
        {
            Id = "post_to_reddit";
            Description = "Post To Reddit";
            InputKeys = new string[] { "clientId", "clientSecret", "subreddit", "title", "text", "redditAppName" };
            OutputKeys = new string[] { "postUrl" };
            Username = username;
            Password = password;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var clientId = ((string)information.Input.Structured["clientId"]).Trim();
            var clientSecret = ((string)information.Input.Structured["clientSecret"]).Trim();
            var subreddit = ((string)information.Input.Structured["subreddit"]).Trim();
            var title = ((string)information.Input.Structured["title"]).Trim();
            var text = ((string)information.Input.Structured["text"]).Trim();
            var redditAppName = ((string)information.Input.Structured["redditAppName"]).Trim();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", redditAppName);

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = Username,
                ["password"] = Password
            });

            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")));

            var tokenResponse = await httpClient.SendAsync(tokenRequest);
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
            var accessToken = tokenData["access_token"];

            var postRequest = new HttpRequestMessage(HttpMethod.Post, $"https://oauth.reddit.com/{subreddit}/api/submit");
            postRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["title"] = title,
                ["text"] = text,
                ["kind"] = "self",
                ["sr"] = subreddit
            });
            postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            postRequest.Headers.Add("User-Agent", "IntegrateWithC#");
            var postResponse = await httpClient.SendAsync(postRequest);
            var postJson = await postResponse.Content.ReadAsStringAsync();
            var postData = JsonConvert.DeserializeObject<Dictionary<string, object>>(postJson);
            var postUrl = postData["url"];
            return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "postUrl", postUrl.ToString() } }));
        }
    }
}