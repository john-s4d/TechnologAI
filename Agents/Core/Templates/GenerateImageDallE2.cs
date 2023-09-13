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

        internal string ApiKey { get; set; } = string.Empty;
        public GenerateImageDallE2(string apiKey)
        {
            Id = "generate_imagedall_e2";
            Description = "Generate Image Dall E";
            InputKeys = new[] { "nImages", "imageSize", "openApiUrl", "organisationId", "msg" };
            OutputKeys = new[] { "contents" };
            ApiKey = apiKey;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var nImages = int.Parse(((string)information.Input.Structured["nImages"]).Trim());
            var imageSize = ((string)information.Input.Structured["imageSize"]).Trim();
            var openApiUrl = ((string)information.Input.Structured["openApiUrl"]).Trim();
            var organisationId = ((string)information.Input.Structured["organisationId"]).Trim();
            var msg = ((string)information.Input.Structured["msg"]).Trim();

            IOpenAIProxy aiClient = new OpenAIHttpService(organisationId, ApiKey, openApiUrl);
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
                        return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "New image saved at {0}", $"'{fullPath}'" } }));
                    }
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", $"Unable to generate image : '{ex.Message}'" } }));
            }
            return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", "image generating failed" } }));
        }
    }
}