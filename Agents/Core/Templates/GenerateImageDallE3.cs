using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Technologai.Agents.Core.DataModels;

namespace Technologai.Templates
{

    /// <summary>
    /// Generate Image Dall E
    /// </summary>
    public class GenerateImageDallE3 : Template
    {
        internal string ApiKey { get; set; } = string.Empty;
        public GenerateImageDallE3(string apiKey)
        {
            Id = "generate_image_dall_e3";
            Description = "Generate Image Dall E using open AI";
            InputKeys = new string[] { "apiUrl", "inputText", "noOfImages:int", "imageSize", "savePath" };
            OutputKeys = new[] { "text[]:fileNames" };
            ApiKey = apiKey;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        { 
            var apiUrl = ((string)information.Input.Structured["apiUrl"]);
            var inputText = ((string)information.Input.Structured["inputText"]);

            int noOfImages = 1;
            int.TryParse((string)information.Input.Structured["noOfImages"], out noOfImages);
            var imageSize = ((string)information.Input.Structured["imageSize"]);

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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<DallEResponseData>(responseContent);
                    int countOfImages = 0;
                    if (responseData != null)
                    {
                        List<string> fileNames = new();

                        foreach (var output in responseData.data)
                        {
                            //Spliting the image name with . extension
                            var imageNameWithExtension = ((string)information.Input.Structured["imageNameWithExtension"]);
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
                            var fullPath = ((string)information.Input.Structured["savePath"]);
                            fullPath += $"{imageNameWithExtension}";
                            await DownloadImage(output.url.ToString(), fullPath);
                            fileNames.Add(fullPath);
                        }
                        return Data.Create("fileNames", fileNames);
                    }
                    return Data.Create("Error", $"Error occured while parsing response");


                }
                else
                {
                    // Display the error message
                    return Data.Create("Error", response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
               return Data.Create(ex);
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