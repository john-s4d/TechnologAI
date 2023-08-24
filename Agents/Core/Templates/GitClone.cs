using System.Diagnostics;

namespace Technologai.Templates
{
    /// <summary>
    /// Git clone from the method
    /// </summary>
    public class GitClone : Template
    {
        public string Description { get; } = "Git clone a repository.";
        public string SampleJsonIn { get; } = "{\"repoLink\":\"string\", \"directory\":\"string\"}";
        public string SampleJsonOut { get; } = string.Empty;

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var reopoLink = ((string)data["repoLink"]).Trim();
            try
            {
                await Task.Run(() =>
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "git",
                            Arguments = $"clone {((string)data["repoLink"]).Trim()}",
                            WorkingDirectory = ((string)data["directory"]).Trim(),
                        }
                    };
                    process.Start();
                });
                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error cloning the given ropo '{reopoLink}': {ex.Message}" } };
            }

        }
    }
}