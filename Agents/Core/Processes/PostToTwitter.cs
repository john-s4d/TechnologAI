using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Technologai
{
    /*
         //PostToTwitter
            PostToTwitter postToTwitter = new();
            var postToTwitterDict = new Dictionary<string, object>()
            {
                {"ConsumerKey","Enter Consumer Key"},
                {"ConsumerKeySecret","Enter Consumer Key Secret"},
                {"AccessToken", "Enter Access Token"},
                {"AccessTokenSecret","Enter Access Token Secret"},
                {"textToPost","Enter the desired text to Post to twitter"}
            };
            var postToTwitterResponse = postToTwitter.Execute(postToTwitterDict).Result; 
    */

    /// <summary>
    /// Post To Twitter
    /// </summary>
    /// 
    public class PostToTwitter : Neuron
    {
        public string Description { get; } = "Post To Twitter";
        public string SampleJsonIn { get; set; } = "{\"ConsumerKey\":\"string\",\"ConsumerKeySecret\":\"string\",\"AccessToken\":\"string\",\"AccessTokenSecret\":\"string\",\"textToPost\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var consumerKey = ((string)data["ConsumerKey"]).Trim();
            var consumerKeySecret = ((string)data["ConsumerKeySecret"]).Trim();
            var accessToken = ((string)data["AccessToken"]).Trim();
            var accessTokenSecret = ((string)data["AccessTokenSecret"]).Trim();
            var tweetText = ((string)data["textToPost"]).Trim();

            var httpMethod = "POST";
            var url = "https://api.twitter.com/2/tweets";
            var timeStamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            var nonce = Guid.NewGuid().ToString();
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timeStamp },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };

            var signature = HttpUtility.UrlEncode(GenerateSignature(httpMethod, url, parameters, consumerKeySecret, accessTokenSecret));
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth",
                $"oauth_consumer_key=\"{consumerKey}\",oauth_token=\"{accessToken}\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"{timeStamp}\",oauth_nonce=\"{nonce}\",oauth_version=\"1.0\",oauth_signature=\"{signature}\"");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var tweet = new { text = tweetText };
            var tweetJson = JsonConvert.SerializeObject(tweet);
            var content = new StringContent(tweetJson, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.twitter.com/2/tweets", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new Dictionary<string, object> { { "contents", responseContent } };
            }
            else
            {
                return new Dictionary<string, object> { { "Errors", $"The request failed with status code: {response.StatusCode}" } };
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