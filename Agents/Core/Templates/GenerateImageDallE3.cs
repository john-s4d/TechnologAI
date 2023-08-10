using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Technologai.Agents.Models;

namespace Technologai
{
    /*
      //GenerateImageDall E
        GenerateImageDallE generateImageDallE = new();
        var generateImageDallE_Dict = new Dictionary<string, object>()
        {
            {"apiKey","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"},
            {"savePath", @"local path to store image"},
            {"imageNameWithExtension","tiger.jpg"},
            {"apiUrl", "https://api.openai.com/v1/images/generations"},
            //A text description of the desired image(s). The maximum length is 1000 characters.
            {"inputText", "a happy and colorful tiger  in forest"},
            //The number of images to generate. Must be between 1 and 10.
            {"noOfImages", "10"},
            //The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024
            {"imageSize","1024x1024"}
        };
        //var generateImageDallEResponse =await generateImageDallE.Execute(generateImageDallE_Dict);

     */

    /// <summary>
    /// Generate Image Dall E
    /// </summary>
    public class GenerateImageDallE3 : Template
    {
        public string Description { get; } = "Generate Image Dall E using open AI";
        public string SampleJsonIn { get; set; } = "{\"apiKey\":\"string\",\"apiUrl\":\"string\",\"inputText\":\"string\",\"noOfImages\":\"int\",\"imageSize\":\"string\",\"savePath\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var apiKey = ((string)data["apiKey"]);
            var apiUrl = ((string)data["apiUrl"]);
            var inputText = ((string)data["inputText"]);

            int noOfImages = 1;
            int.TryParse((string)data["noOfImages"], out noOfImages);
            var imageSize = ((string)data["imageSize"]);

            var requestData = new DallERequestData()
            {
                InputText = inputText,
                NoOfImages = noOfImages,
                ImageSize = imageSize
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            // Generate image
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<DallEResponseData>(responseContent);
                    int countOfImages = 0;
                    if (responseData != null)
                    {
                        foreach (var output in responseData.data)
                        {
                            //Spliting the image name with . extension
                            var imageNameWithExtension = ((string)data["imageNameWithExtension"]);
                            string[] str = imageNameWithExtension.Split(".");
                            string imageName = string.Empty;
                            string dynamicImageName = string.Empty;
                            for (int i = 0; i < 1; i++)
                            {
                                imageName = str[0];
                            }

                            countOfImages++;

                            //Cresating dynamic image name
                            dynamicImageName = imageName + $"{countOfImages}";
                            imageNameWithExtension = imageNameWithExtension.Replace(imageName, dynamicImageName);
                            //reading local system path
                            var savePath = ((string)data["savePath"]);
                            savePath += $"{imageNameWithExtension}";
                            await DownloadImage(output.url.ToString(), savePath);

                        }
                        return new Dictionary<string, object> { { "contents", responseData.data } };
                    }
                    else
                    {
                        return new Dictionary<string, object> { { "Error", $"Error occured while parsing response" } };
                    }
                }
                else
                {
                    // Display the error message
                    return new Dictionary<string, object> { { "Error", $"'{response.ReasonPhrase}'" } };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "Error", $"Unable to generate image : '{ex.Message}'" } };
            }

            //Download Image
            static async Task DownloadImage(string imageUrl, string savePath)
            {
                using HttpClient client = new();
                var imageBytes = await client.GetByteArrayAsync(imageUrl);
                // Save the image to the specified path
                File.WriteAllBytes(savePath, imageBytes);
            }
        }
    }
}