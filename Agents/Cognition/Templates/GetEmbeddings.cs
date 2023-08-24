using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Technologai.Agents.Cognition.DataModels;

namespace Technologai
{
    /// <summary>
    /// GetEmbeddings
    /// </summary>
    public class GetEmbeddings : Template
    {
        /*
          //Get Embeddings
        GetEmbeddings getEmbeddings = new();
        var getEmbeddings_Dict = new Dictionary<string, object>()
        {
            {"apiKey", "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"},
            {"apiUrl", "https://api.openai.com/v1/embeddings"},
            {"inputText", "Your text string goes here"},
            {"embeddingsModel", "text-embedding-ada-002"}
        };
        //var getEmbeddingsResponse = await getEmbeddings.Execute(getEmbeddings_Dict);
         */

        public string Description { get; } = "Get Embeddings using open AI api key";
        public string SampleJsonIn { get; set; } = "{\"apiKey\":\"string\",\"inputText\":\"string\",\"apiUrl\":\"string\",\"embeddingsModel\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var apiKey = (string)data["apiKey"];
            var apiUrl = (string)data["apiUrl"];
            var inputText = (string)data["inputText"];
            var embeddingsModel = (string)data["embeddingsModel"];

            var requestData = new EmbeddingsRequestDataModel()
            {
                Input = inputText,
                Model = embeddingsModel,
            };
            var jsonData = JsonSerializer.Serialize(requestData);

            // Get Embeddings
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                // Handle the response
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Parse the response to extract the image URL
                    var responseData = JsonSerializer.Deserialize<EmbeddingsRequestDataModel>(responseContent);

                    if (responseData != null)
                    {
                        return new Dictionary<string, object> { { "contents", responseData } };
                    }
                    else
                    {
                        return new Dictionary<string, object> { { "Error", $"Error occured while parsing response" } };
                    }
                }
                else
                {
                    return new Dictionary<string, object> { { "Error", $"Unable to get embeddings : '{response.ReasonPhrase}'" } };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "Exception", $"Something went wrong  : '{ex.Message}'" } };
            }
        }
    }
}