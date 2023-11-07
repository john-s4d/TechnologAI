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
            InputKeys = new string[] { "repoLink:Uri", "directory" };
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
                return null;
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}