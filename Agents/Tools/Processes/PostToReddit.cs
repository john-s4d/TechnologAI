using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Technologai
{
    /// <summary>
    /// Post To Reddit
    /// </summary>
    public class PostToReddit : Process
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

        public string Description { get; } = "Post To Reddit";
        public string SampleJsonIn { get; set; } = "{\"clientId\":\"string\",\"clientSecret\":\"string\",\"username\":\"string\",\"password\":\"string\",\"subreddit\":\"string\",\"title\":\"string\",\"text\":\"string\",\"redditAppName\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"postUrl\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var clientId = ((string)data["clientId"]).Trim();
            var clientSecret = ((string)data["clientSecret"]).Trim();
            var username = ((string)data["username"]).Trim();
            var password = ((string)data["password"]).Trim();
            var subreddit = ((string)data["subreddit"]).Trim();
            var title = ((string)data["title"]).Trim();
            var text = ((string)data["text"]).Trim();
            var redditAppName = ((string)data["redditAppName"]).Trim();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", redditAppName);

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password
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
            return new Dictionary<string, object> { { "postUrl", postUrl } };
        }
    }
}