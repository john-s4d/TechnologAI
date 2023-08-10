namespace Technologai.Agents
{
    internal class ReadLocalFile : Template
    {
        public new string Description { get; } = "Read a text file on the local filesystem.";
        public new string SampleJsonIn { get; } = "{\"filename\":\"string\"}";
        public new string SampleJsonOut { get; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            if (data == null || !data.TryGetValue("filename", out object filenameObj) || !(filenameObj is string filename))
            {
                return new Dictionary<string, object> { { "error", $"Invalid or missing filename in input data." } };
            }

            try
            {
                using StreamReader reader = new StreamReader(filename);
                var contents = await reader.ReadToEndAsync();
                return new Dictionary<string, object> { { "contents", contents } };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error reading file '{filename}': {ex.Message}" } };
            }
        }
    }
}