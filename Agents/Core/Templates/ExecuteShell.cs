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
            Description = "Execute Shell in the local system.";
            InputKeys = new string[] { "fileName", "arguments" };
            OutputKeys = new string[] { "output"};
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            try
            {
                var output = await Task.Run(() =>
                {
                    System.Diagnostics.Process process = new();

                    // Configure the process to run the command
                    process.StartInfo.FileName = ((string)information.Input.Structured?["fileName"]).Trim();
                    process.StartInfo.Arguments = ((string)information.Input.Structured?["arguments"]).Trim();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;

                    // Start the process and wait for it to complete
                    process.Start();
                    process.WaitForExit();

                    // Read the output from the command and print it to the console
                    string output = process.StandardOutput.ReadToEnd();

                    return output;
                });

                if (output != string.Empty)
                {
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "output", output } }));
                }
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> ()));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error occured while processing request: {ex.Message}" } }));
            }
        }    
    }
}