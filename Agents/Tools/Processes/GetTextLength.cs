namespace Technologai
{
    /// <summary>
    /// Get text length.
    /// </summary>
    internal class GetTextLength : Process
    {
        public string Description { get; } = "Get text length.";
        public string SampleJsonIn { get; } = "{\"text\":\"string\"}";
        public string SampleJsonOut { get; } = "{\"length\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var length = await Task.Run(() =>
            {
                return ((string)data["text"]).Length;
            });

            return new Dictionary<string, object>() { { "length", length } };
        }
    }
}