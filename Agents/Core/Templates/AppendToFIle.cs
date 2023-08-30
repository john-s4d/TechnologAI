namespace Technologai.Templates
{
    internal class AppendToFile : Template
    {
        public AppendToFile()
        {
            Id = "append_to_file";
            Description = "Append text to file in the local filesystem.";
            InputKeys = new string[] { "filename", "content" };            
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            using (var writer = new StreamWriter(information.Input?.Structured?["filename"]
                       ?? throw new ArgumentNullException("filename"), true))
            {
                await writer.WriteAsync(information.Input?.Structured?["content"]);
            }
            return null;
        }
    }
}