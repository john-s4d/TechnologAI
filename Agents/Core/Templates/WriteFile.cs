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
                        return Data.Create("error", $"File already exists: '{fileName}'");
                    }
                }

                using StreamWriter writer = new(fileName);
                await writer.WriteAsync((string)information.Input.Structured["content"]);
                return null;
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}