namespace Technologai.Templates
{
    /// <summary>
    /// Write a text file on the local filesystem.
    /// </summary>
    internal class WriteFile : Template
    {
        public WriteFile()
        {
            Id = "write_file";
            Description = "Write a text file on the local filesystem.";
            InputKeys = new string[] { "fileName", "content", "overrideIfExists" };
            OutputKeys = new string[] { };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            string fileName = ((string)information.Input.Structured["fileName"]).Trim();
            try
            {
                var overrideIfExists = information.Input.Structured["overrideIfExists"];
                if (overrideIfExists == null || (overrideIfExists != null && Convert.ToBoolean(overrideIfExists) == false))
                {
                    if (File.Exists(fileName))
                    {
                        return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"File already exists." } }));
                    }
                }

                using StreamWriter writer = new(fileName);
                await writer.WriteAsync((string)information.Input.Structured["content"]);
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> ()));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } }));
            }
        }
    }
}