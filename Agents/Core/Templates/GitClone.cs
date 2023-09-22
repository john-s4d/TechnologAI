using System.Diagnostics;

namespace Technologai.Templates
{
    /// <summary>
    /// Git clone from the method
    /// </summary>
    public class GitClone : Template
    {
        public GitClone()
        {
            Id = "git_clone";
            Description = "Git clone a repository.";
            InputKeys = new string[] { "repoLink", "directory" };
            OutputKeys = new string[] { };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var reopoLink = ((string)information.Input.Structured?["repoLink"]).Trim();
            try
            {
                await Task.Run(() =>
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "git",
                            Arguments = $"clone {((string)information.Input.Structured?["repoLink"]).Trim()}",
                            WorkingDirectory = ((string)information.Input.Structured?["directory"]).Trim(),
                        }
                    };
                    process.Start();
                });
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> () ));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error cloning the given ropo '{reopoLink}': {ex.Message}" } }));
            }
        }
    }
}