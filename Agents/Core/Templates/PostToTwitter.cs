
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Technologai.Templates
{
 

    /// <summary>
    /// Post To Twitter
    /// </summary>
    /// 
    public class PostToTwitter : Template
    {
        internal string ConsumerKey { get; set; } = null!;
        internal string ConsumerKeySecret { get; set; } = null!;
        internal string AccessToken { get; set; } = null!;
        internal string AccessTokenSecret { get; set; } = null!;

        public PostToTwitter(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            Id = "post_to_twitter";
            Description = "Post To Twitter";
            InputKeys = new string[] { "textToPost" };
            OutputKeys = new string[] { "result" };

            ConsumerKey = consumerKey;
            ConsumerKeySecret = consumerKeySecret;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var tweetText = (information.Input.Structured?["textToPost"]).Trim();
                var httpMethod = "POST";
                var url = "https://api.twitter.com/2/tweets";
                var timeStamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                var nonce = Guid.NewGuid().ToString();
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "oauth_consumer_key", ConsumerKey },
                    { "oauth_nonce", nonce },
                    { "oauth_signature_method", "HMAC-SHA1" },
                    { "oauth_timestamp", timeStamp },
                    { "oauth_token", AccessToken },
                    { "oauth_version", "1.0" }
                };

                var signature = HttpUtility.UrlEncode(GenerateSignature(httpMethod, url, parameters, ConsumerKeySecret, AccessTokenSecret));
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth",
                    $"oauth_consumer_key=\"{ConsumerKey}\",oauth_token=\"{AccessToken}\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"{timeStamp}\",oauth_nonce=\"{nonce}\",oauth_version=\"1.0\",oauth_signature=\"{signature}\"");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var tweet = new { text = tweetText };
                var tweetJson = JsonConvert.SerializeObject(tweet);
                var content = new StringContent(tweetJson, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://api.twitter.com/2/tweets", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Data.Create(result);
                }
                else
                {
                    return Data.Create("Errors", $"The request failed with status code: {response.StatusCode}");
                }
            }
            catch(Exception ex) 
            { 
                return Data.Create(ex);
            }
        }
        public static string GenerateSignature(string httpMethod, string url, IDictionary<string, string> parameters, string consumerSecret, string tokenSecret = null)
        {
            var signatureBaseString = GenerateSignatureBaseString(httpMethod, url, parameters);
            var signingKey = GenerateSigningKey(consumerSecret, tokenSecret);
            byte[] signatureBaseStringBytes = Encoding.UTF8.GetBytes(signatureBaseString);
            using (HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey)))
            {
                byte[] signatureBytes = hmacsha1.ComputeHash(signatureBaseStringBytes);
                return Convert.ToBase64String(signatureBytes);
            }
        }
        private static string GenerateSignatureBaseString(string httpMethod, string url, IDictionary<string, string> parameters)
        {
            List<string> parameterStrings = new List<string>();
            foreach (var parameter in parameters)
            {
                parameterStrings.Add(parameter.Key + "=" + Uri.EscapeDataString(parameter.Value));
            }
            parameterStrings.Sort();
            var normalizedParameters = string.Join("&", parameterStrings);
            var signatureBaseString = string.Format("{0}&{1}&{2}", httpMethod.ToUpperInvariant(), Uri.EscapeDataString(url), Uri.EscapeDataString(normalizedParameters));
            return signatureBaseString;
        }
        private static string GenerateSigningKey(string consumerSecret, string tokenSecret = null)
        {
            var key = Uri.EscapeDataString(consumerSecret) + "&";
            if (!string.IsNullOrEmpty(tokenSecret))
            {
                key += Uri.EscapeDataString(tokenSecret);
            }
            return key;
        }
    }
}