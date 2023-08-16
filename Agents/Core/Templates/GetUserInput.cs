namespace Technologai
{
    /// <summary>
    /// Take a response from the user.
    /// </summary>
    public class GetUserInput : Template
    {
        public string Description { get; set; } = "Receive a response from the user.";
        public string SampleJsonIn { get; set; } = string.Empty;
        public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            var value = await Task.Run(() =>
            {
                return Console.ReadLine() ?? string.Empty;
            });

            return new Dictionary<string, object> { { "output", value } };
        }
    }
}