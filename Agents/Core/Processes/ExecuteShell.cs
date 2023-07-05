namespace Technologai
{
    /// <summary>
    /// Execute Shell
    /// </summary>
    public class ExecuteShell : Neuron
    {
        public string Description { get; } = "Execute Shell in the local system.";
        public string SampleJsonIn { get; set; } = "{\"fileName\":\"string\", \"arguments\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            try
            {
                var output = await Task.Run(() =>
                {
                    System.Diagnostics.Process process = new();

                    // Configure the process to run the command
                    process.StartInfo.FileName = ((string)data["fileName"]).Trim();
                    process.StartInfo.Arguments = ((string)data["arguments"]).Trim();
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
                    return new Dictionary<string, object> { { "output", output } };
                }
                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error occured while processing request: {ex.Message}" } };
            }
        }
    }
}