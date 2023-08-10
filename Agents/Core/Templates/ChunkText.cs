namespace Technologai.Agents.Core.Templates
{
    /// <summary>
    /// Get chunk text.
    /// </summary>
    internal class ChunkText : Template
    {
        public string Description { get; } = "Get chunk text.";
        public string SampleJsonIn { get; } = "{\"text\":\"string\", \"chunkSize\":\"string\"}";
        public string SampleJsonOut { get; } = "{\"content\":\"string[]\"}";

        public async Task<Dictionary<string, object>> Spike(Dictionary<string, object> data)
        {
            var response = new Dictionary<string, object>();
            // text from the user input
            string text = ((string)data["text"]);

            // chunk size in integer
            int chunkSize = int.Parse(((string)data["chunkSize"]));
            var listOfChunk = new List<object>();
            if (string.IsNullOrEmpty(text))
                return new Dictionary<string, object> { { "error", $"Entered text in null or empty : '{text}' " } };
            await Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < text.Length; i += chunkSize)
                    {
                        int length = Math.Min(chunkSize, text.Length - i);
                        string chunk = text.Substring(i, length);
                        listOfChunk.Add(chunk);
                    }
                    response.Add("content", listOfChunk);
                }
                catch (Exception ex)
                {
                    response.Add("error", $"error chunking the text '{text}': '{ex.Message}'");
                }
            });
            return response;
        }
    }
}