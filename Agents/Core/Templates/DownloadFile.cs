using System;
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
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);
        public override async Task<Data?> Process(Information information)
        {
            var url = new Uri($"{((string)information?.Input?.Structured?["weburl"]).Trim()}");
            string filePath = ((string)information?.Input.Structured?["filepath"]).Trim();

            try
            {
                // Assuming this code is inside an async method
                await Task.Run(async () =>
                {
                    using (HttpClient client = new())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode(); // Ensure a successful response
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                });
            }
            catch (WebException ex)
            {
                return Data.Create(ex);
            }
            return null;
        }
    }
}