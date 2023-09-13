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
            Description = "Execute Python";
            InputKeys = new string[] { "cmd", "args" };
            OutputKeys = new string[] { "output" };  
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var output = await Task.Run(() =>
                {
                    ProcessStartInfo start = new()
                    {
                        FileName = information.Input.Structured?["cmd"].ToString(),
                        Arguments = information.Input.Structured?["args"].ToString(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                    using Process? process = System.Diagnostics.Process.Start(start);
                    using StreamReader? reader = process?.StandardOutput;
                    string? output = reader?.ReadToEnd();

                    return output;
                });

                if (!string.IsNullOrEmpty(output))
                {
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "output", output } }));
                }
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error occured while executing python cmd : {ex.Message}" } }));
            }
        }
    }
}