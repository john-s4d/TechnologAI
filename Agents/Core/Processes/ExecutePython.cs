using System.Diagnostics;

namespace Technologai
{
    /// <summary>
    /// Execute python
    /// </summary>
    public class ExecutePython : Process
    {
        public string Description { get; } = "Execute python in the local system.";
        public string SampleJsonIn { get; } = "{\"cmd\":\"string\",\"args\":\"string\" }";
        public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            try
            {
                var output = await Task.Run(() =>
                {
                    ProcessStartInfo start = new()
                    {
                        FileName = data["cmd"].ToString(),
                        Arguments = data["args"].ToString(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                    using System.Diagnostics.Process process = System.Diagnostics.Process.Start(start);
                    using StreamReader reader = process.StandardOutput;
                    string output = reader.ReadToEnd();

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
                return new Dictionary<string, object> { { "error", $"Error occured while executing python cmd : {ex.Message}" } };
            }
        }
    }
}