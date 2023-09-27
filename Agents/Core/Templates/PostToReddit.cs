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
        
        internal string Username { get; set; } = null!;
        internal string Password { get; set; } = null!;
        internal string ClientId { get; set; } = null!;
        internal string ClientSecret { get; set; } = null!;
        internal string SubReddit { get; set; } = null!;
        internal string RedditAppName { get; set; } = null!;

        public PostToReddit(string username, string password, string clientId, string clientSecret, string subReddit, string redditAppName)
        {
            Id = "post_to_reddit";
            Description = "Post To Reddit";
            InputKeys = new string[] {  "title", "text" };
            OutputKeys = new string[] { "object:postUrl" };
            Username = username;
            Username = username;
            Password = password;
            ClientId = clientId;
            ClientSecret = clientSecret;
            SubReddit = subReddit;
            RedditAppName = redditAppName;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var title = ((string)information.Input.Structured["title"]).Trim();
                var text = ((string)information.Input.Structured["text"]).Trim();

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", RedditAppName);

                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
                tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = Username,
                    ["password"] = Password
                });

                tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}")));

                var tokenResponse = await httpClient.SendAsync(tokenRequest);
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
                var accessToken = tokenData["access_token"];

                var postRequest = new HttpRequestMessage(HttpMethod.Post, $"https://oauth.reddit.com/{SubReddit}/api/submit");
                postRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["title"] = title,
                    ["text"] = text,
                    ["kind"] = "self",
                    ["sr"] = SubReddit
                });
                postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                postRequest.Headers.Add("User-Agent", "IntegrateWithC#");
                var postResponse = await httpClient.SendAsync(postRequest);
                var postJson = await postResponse.Content.ReadAsStringAsync();
                var postData = JsonConvert.DeserializeObject<Dictionary<string, object>>(postJson);
                var postUrl = postData["url"];
                return Data.Create(postUrl.ToString());
            }
            catch(Exception ex) 
            { 
                return Data.Create(ex);
            }
        }
    }
}