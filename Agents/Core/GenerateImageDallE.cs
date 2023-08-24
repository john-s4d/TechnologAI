using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Technologai.Agents.Core.DataModels;

namespace Core
{
    public class GenerateImageDallE
    {
        public interface IOpenAIProxy
        {
            //Send the Prompt Text with and return a list of image URLs
            Task<GenerateImageResponse> GenerateImages(GenerateImageRequest prompt, CancellationToken cancellation = default);

            //Download the Image as byte array
            Task<byte[]> DownloadImage(string url);
        }
        public class OpenAIHttpService : IOpenAIProxy
        {
            readonly HttpClient _httpClient;

            readonly string _subscriptionId;

            readonly string _apiKey;

            public OpenAIHttpService(string _subscriptionId, string _apiKey, string openApiUrl)
            {
                _httpClient = new HttpClient { BaseAddress = new Uri(openApiUrl) };
                this._subscriptionId = _subscriptionId;
                this._apiKey = _apiKey;
            }
            public async Task<GenerateImageResponse> GenerateImages(GenerateImageRequest prompt, CancellationToken cancellation = default)
            {
                using var rq = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations");
                var jsonRequest = JsonSerializer.Serialize(prompt, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                //serialize the content to JSON and set the correct content type
                rq.Content = new StringContent(jsonRequest);
                rq.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Including the Authorization Header with API Key
                var apiKey = _apiKey;
                rq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // Including the Subscription Id Header
                var subscriptionId = _subscriptionId;
                rq.Headers.TryAddWithoutValidation("OpenAI-Organization", subscriptionId);
                var response = await _httpClient.SendAsync(rq, HttpCompletionOption.ResponseHeadersRead, cancellation);
                response.EnsureSuccessStatusCode();
                var content = response.Content;
                var jsonResponse = await content.ReadFromJsonAsync<GenerateImageResponse>(cancellationToken: cancellation);
                return jsonResponse;
            }
            public async Task<byte[]> DownloadImage(string url)
            {
                var buffer = await _httpClient.GetByteArrayAsync(url);
                return buffer;
            }
        }
    }
}
