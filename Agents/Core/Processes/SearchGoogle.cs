using Newtonsoft.Json.Linq;

namespace Technologai
{
    /// <summary>
    /// Search Google
    /// </summary>
    public class SearchGoogle : Neuron
    {
        public string Description { get; } = "Search Google using serach query";
        public string SampleJsonIn { get; set; } = "{\"apikey\":\"string\",\"searchEngineID\":\"string\",\"searchQuery\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

        private string ApiKey { get; set; }

        public SearchGoogle(string apiKey)
        {
            ApiKey = apiKey;
        }

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var searchEngineId = ((string)data["searchEngineID"]).Trim(); ;

            // Define the search query
            var query = ((string)data["searchQuery"]).Trim();

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

                    return new Dictionary<string, object>() { { "result", jsonObject } };
                }
                else
                {
                    return new Dictionary<string, object>();
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error executing request on google: {ex.Message}" } };
            }
        }
    }
}