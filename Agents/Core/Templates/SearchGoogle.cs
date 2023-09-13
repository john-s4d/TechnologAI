using Newtonsoft.Json.Linq;

namespace Technologai.Templates
{
    /// <summary>
    /// Search Google
    /// </summary>
    public class SearchGoogle : Template
    {
        readonly public string ApiKey;
        public SearchGoogle(string apiKey)
        {
            Id = "search_google";
            Description = "Search Google using serach query";
            InputKeys = new string[] {"searchEngineID", "searchQuery" };
            OutputKeys = new string[] { "output" };
            ApiKey = apiKey;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var searchEngineId = ((string)information.Input.Structured["searchEngineID"]).Trim(); ;

            // Define the search query
            var query = ((string)information.Input.Structured["searchQuery"]).Trim();

            // Create a new instance of HttpClient to send HTTP requests
            var httpClient = new HttpClient();

            try
            {
                // Send a GET request to the Google API to perform the search
                var url = $"https://www.googleapis.com/customsearch/v1?key={ApiKey}&cx={searchEngineId}&q={query}";
                var response = await httpClient.GetAsync(url);
                if (response != null)
                {
                    // Read the content of the response as a string
                    var jsonString = response.Content.ReadAsStringAsync().Result;

                    // Parse the JSON response using Newtonsoft.Json
                    var jsonObject = JObject.Parse(jsonString);

                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "result", jsonObject.ToString() } }));
                }
                else
                {
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> ()));
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error executing request on google: {ex.Message}" } }));
            }
        }
    }
}