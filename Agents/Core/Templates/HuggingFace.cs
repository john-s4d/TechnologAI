using System;
using System.Threading.Tasks;

namespace Technologai.Agents.Core
{

    class HuggingFace
    {

        public class HuggingFaceAPI
        {
            /*
            // Instantiate the HuggingFaceAPI class
            var huggingFaceAPI = new HuggingFaceAPI();

            // Enter the text input for generating the image
            Console.Write("Enter the text input: ");
            string textInput = Console.ReadLine();

            try
            {
                // Call the GenerateImageAsync method and wait for the response
                var response = await huggingFaceAPI.GenerateImageAsync(textInput);

            // Display the generated image URL
            Console.WriteLine("Generated Image URL:");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            */

            private const string APIUrl = "https://api-inference.huggingface.co/models/distilgpt2/generate";
            private const string AccessToken = "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";

            private readonly HttpClient httpClient;

            public HuggingFaceAPI()
            {
                httpClient = new HttpClient();
            }

            public async Task<string> GenerateImageAsync(string text)
            {
                // Set the authorization header with your Hugging Face API token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "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");

                // Set the request content type and body
                var content = new StringContent("{\"inputs\":\"" + text + "\"}", System.Text.Encoding.UTF8, "application/json");

                // Make the POST request to the API
                var response = await httpClient.PostAsync(APIUrl, content);

                // Read the response content
                var responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }

        }
       

    }
}