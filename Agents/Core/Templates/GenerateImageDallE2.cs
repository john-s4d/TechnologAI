using Technologai.Agents.Core.DataModels;
using static Core.GenerateImageDallE;

namespace Technologai.Templates
{
    /// <summary>
    /// Generate Image DaLL E
    /// </summary>
    public class GenerateImageDallE2 : Template
    {

        /*
        //GenerateImageDallE
        GenerateImageDallE generateImageDallE = new();
        var generateInageDallEDict = new Dictionary<string, object>()
         {
             {"nImages","1"},
             {"imageSize","1024x1024" },
             {"openApiUrl",@"https://api.openai.com" },
             {"organisationId","organisation ID"},
             {"apiKey","api key"},
             {"msg","enter text"},
         };
        var reponseByGenerateImageDallE = generateImageDallE.Execute(generateInageDallEDict).Result;
        */

        public string Description { get; } = "Generate Image Dall E";
        public string SampleJsonIn { get; set; } = "{\"nImages\":\"int\",\"imageSize\":\"string\",\"openApiUrl\":\"string\",\"organisationId\":\"string\",\"apiKey\":\"string\",\"msg\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"contents\":\"object\"}";
        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var nImages = int.Parse(((string)data["nImages"]).Trim());
            var imageSize = ((string)data["imageSize"]).Trim();
            var openApiUrl = ((string)data["openApiUrl"]).Trim();
            var organisationId = ((string)data["organisationId"]).Trim();
            var apiKey = ((string)data["apiKey"]).Trim();
            var msg = ((string)data["msg"]).Trim();

            IOpenAIProxy aiClient = new OpenAIHttpService(organisationId, apiKey, openApiUrl);
            try
            {
                var prompt = new GenerateImageRequest(msg, nImages, imageSize);
                var result = await aiClient.GenerateImages(prompt);
                if (result != null)
                {
                    foreach (var item in result.Data)
                    {
                        Console.WriteLine(item.Url);
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid()}.png");
                        var img = await aiClient.DownloadImage(item.Url);
                        await File.WriteAllBytesAsync(fullPath, img);
                        return new Dictionary<string, object> { { "New image saved at {0}", $"'{fullPath}'" } };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "Error", $"Unable to generate image : '{ex.Message}'" } };
            }
            return new Dictionary<string, object> { { "contents", "image generating failed" } };
        }
    }
}