namespace Technologai
{
    /// <summary>
    /// Write a text file on the local filesystem.
    /// </summary>
    internal class WriteFile : Process
    {
        public new string Description { get; } = "Write a text file on the local filesystem.";
        public  string SampleJsonIn { get; } = "{\"fileName\":\"string\", \"content\":\"string\", \"overrideIfExists\":\"bool\"}";// optional: overrideIfExists
        public  string SampleJsonOut { get; } = string.Empty;

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            string fileName = ((string)data["fileName"]).Trim();
            try
            {
                data.TryGetValue("overrideIfExists", out object overrideIfExists);
                if (overrideIfExists == null || (overrideIfExists != null && (bool)overrideIfExists == false))
                {
                    if (File.Exists(fileName))
                    {
                        return new Dictionary<string, object> { { "error", $"File already exists." } };
                    }
                }

                using StreamWriter writer = new(fileName);
                await writer.WriteAsync((string)data["content"]);
                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } };
            }
        }
    }
}