using Microsoft.VisualBasic;

namespace Technologai.Templates
{
    public class DownloadFile : Template
    {
        public DownloadFile()
        {
            Id = "download_file";
            Description = "Download file";
            InputKeys = new string[] { "filename" };
            OutputKeys = new string[] { "contents" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var filenameObj = information.Input.Structured?["filename"];
            if (information.Input == null || filenameObj != null || !(filenameObj is string filename))
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Invalid or missing filename in input data." } }));
            }
            try
            {
                using StreamReader reader = new StreamReader(filename);
                var contents = await reader.ReadToEndAsync();
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", contents } }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error reading file '{filename}': {ex.Message}" } }));
            }
        }
    }
}