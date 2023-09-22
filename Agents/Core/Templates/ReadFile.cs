namespace Technologai.Templates
{
    /// <summary>
    /// Read a text file on the local filesystem.
    /// </summary>
    internal class ReadFile : Template
    {
        public ReadFile()
        {
            Id = "read_file";
            Description = "Read a text file on the local filesystem.";
            InputKeys = new string[] { "fileName" };
            OutputKeys = new string[] {"contents"};
        }


        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            string fileName = ((string)information.Input.Structured["fileName"]).Trim();
            try
            {
                using StreamReader reader = new(fileName);
                var contents = await reader.ReadToEndAsync();
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "contents", contents } }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error reading file '{fileName}': {ex.Message}" } }));
            }
        }
    }
}