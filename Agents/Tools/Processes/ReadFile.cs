namespace Technologai
{
    /// <summary>
    /// Read a text file on the local filesystem.
    /// </summary>
    internal class ReadFile : Process
    {
        public string Description { get; } = "Read a text file on the local filesystem.";
        public string SampleJsonIn { get; } = "{\"fileName\":\"string\"}";
        public string SampleJsonOut { get; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            string fileName = ((string)data["fileName"]).Trim();
            try
            {
                using StreamReader reader = new(fileName);
                var contents = await reader.ReadToEndAsync();
                return new Dictionary<string, object> { { "contents", contents } };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error reading file '{fileName}': {ex.Message}" } };
            }
        }
    }
}