using System.Diagnostics;

namespace Technologai.Templates
{
    /// <summary>
    /// Execute Shell
    /// </summary>
    public class ExecuteShell : Template
    {
        public ExecuteShell()
        {
            Id = "execute_shell";
            Description = "Execute a shell process on the local system.";
            InputKeys = new string[] { "cmd", "args" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        Process process = new()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = information.Input?.Structured?["cmd"].Trim(),
                                Arguments = information.Input?.Structured?["args"].Trim(),
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                            }
                        };

                        // Start the process and wait for it to complete
                        process.Start();
                        process.WaitForExit();

                        // Read the output from the command and print it to the console
                        return process.StandardOutput.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                    
                });
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}