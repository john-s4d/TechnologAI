namespace Technologai
{

    /// <summary>
    /// Append to a text file on the local filesystem.
    /// </summary>
    internal class AppendToFile : Process
    {
        public string Description { get; } = "Append text to file on the local filesystem.";
        public string SampleJsonIn { get; } = "{\"fileName\":\"string\", \"content\":\"string\", \"createIfNotExists\":\"bool\"}";// optional: createIfNotExists
        public string SampleJsonOut { get; } = string.Empty;

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            string fileName = ((string)data["fileName"]).Trim();
            try
            {
                data.TryGetValue("createIfNotExists", out object createIfNotExists);
                if (createIfNotExists == null || (createIfNotExists != null && (bool)createIfNotExists == false))
                {
                    if (!File.Exists(fileName))
                    {
                        return new Dictionary<string, object> { { "error", $"File doesn't exists." } };
                    }
                }

                using StreamWriter writer = new(fileName, true);
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