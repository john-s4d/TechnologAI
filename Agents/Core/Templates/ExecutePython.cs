using System.Diagnostics;

namespace Technologai.Templates
{
    /// <summary>
    /// Execute python
    /// </summary>
    public class ExecutePython : Template
    {
        public ExecutePython()
        {
            Id = "execute_python";
            Description = "Execute a Python script.";
            InputKeys = new string[] { "cmd", "args" };            
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var output = await Task.Run(() =>
                {
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = "python"; // or "python3" depending on your installation
                    start.Arguments = information.Input?.Structured?["cmd"] + " " + information.Input?.Structured?["args"]; // TODO: Sanitize input
                    start.UseShellExecute = false;
                    start.RedirectStandardOutput = true;
                    using (Process? process = System.Diagnostics.Process.Start(start))
                    {
                        using (StreamReader? reader = process?.StandardOutput)
                        {
                            return reader?.ReadToEnd();                            
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
            return null;
        }
    }
}