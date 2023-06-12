using System.Net;

namespace Technologai
{
    /// <summary>
    /// Download file from web url
    /// </summary>
    public class DownloadFile : Process
    {
        public string Description { get; } = "Downloaded a file to the local filesystem.";
        public string SampleJsonIn { get; } = "{\"weburl\":\"string\",\"filepath\":\"string\" }";
        public string SampleJsonOut { get; } = string.Empty;

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var url = new Uri($"{((string)data["weburl"]).Trim()}");
            string filePath = ((string)data["filepath"]).Trim();
            try
            {
                await Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(url, filePath);
                    };
                });

                return new Dictionary<string, object>();
            }
            catch (WebException ex)
            {
                return new Dictionary<string, object> { { "error", $"Error downloading file '{url}': {ex.Message}" } };
            }
        }
    }
}