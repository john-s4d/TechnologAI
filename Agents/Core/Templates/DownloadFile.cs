using System.Net;

namespace Technologai.Templates
{
    /// <summary>
    /// Download file from web url
    /// </summary>
    public class DownloadFile : Template
    {

        public DownloadFile()
        {
            Id = "download_File";
            Description = "Download a file from the web to the local filesystem.";
            InputKeys = new string[] { "weburl", "filepath" };
            OutputKeys = new string[] { "output" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);
        public override async Task<Data?> Process(Information information)
        {
            var url = new Uri($"{((string)information.Input.Structured?["weburl"]).Trim()}");
            string filePath = ((string)information.Input.Structured?["filepath"]).Trim();
            try
            {
                await Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(url, filePath);
                    };
                });

                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "output", string.Empty } }));
            }
            catch (WebException ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error downloading file '{url}': {ex.Message}" } }));
            }
        }
    }
}