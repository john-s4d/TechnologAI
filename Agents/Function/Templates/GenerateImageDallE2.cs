using System.Text.Json;
using Technologai.Agents.Core.DataModels;
using static Technologai.Agents.Core.GenerateImageDallE;

namespace Technologai.Templates.Core
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

        private string _apiKey;
        private string _organizationId;
        private string _openApiUrl;

        public GenerateImageDallE2(string openApiUrl, string organizationId, string apiKey)
        {
            Id = "generate_imagedall_e2";
            Description = "Generate Image Dall E";
            InputKeys = new[] { "integer:imageCount", "imageSize:int", "prompt" };            
            OutputKeys = new[] { "text[]:fileNames" };

            // TODO: Validate input values according to key type definition

            _apiKey = apiKey;
            _organizationId = organizationId;
            _openApiUrl = openApiUrl;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var imageCount = int.Parse(information.Input?.Structured?["imageCount"]?.Trim() ?? string.Empty);
            var imageSize = information.Input?.Structured?["imageSize"].Trim() ?? string.Empty;            
            var prompt = information.Input?.Structured?["prompt"].Trim() ?? string.Empty;

            IOpenAIProxy aiClient = new OpenAIHttpService(_organizationId, _apiKey, _openApiUrl);
            try
            {
                var request = new GenerateImageRequest(prompt, imageCount, imageSize);
                var result = await aiClient.GenerateImages(request);
                if (result != null)
                {
                    List<string> fileNames = new();

                    foreach (var item in result.Data)
                    {
                        Console.WriteLine(item.Url);
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid()}.png");
                        var img = await aiClient.DownloadImage(item.Url);
                        await File.WriteAllBytesAsync(fullPath, img);
                        fileNames.Add(fullPath);
                    }
                    return Data.Create("fileNames", fileNames);
                }
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }

            return null;
        }
    }
}