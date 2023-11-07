using Newtonsoft.Json.Linq;
using System.Text.Json;

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
            OutputKeys = new string[] { "content" };
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
                    var content = await response.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}