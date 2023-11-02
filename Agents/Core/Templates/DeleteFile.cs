namespace Technologai.Templates
{
    /// <summary>
    /// Delete a file from the local filesystem.
    /// </summary>
    public class DeleteFile : Template
    {

        public DeleteFile()
        {
            Id = "delete_file";
            Description = "Delete a text file on the local filesystem";
            InputKeys = new string[] { "fileName" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            string fileName = (information.Input?.Structured?["fileName"])?.Trim() ?? string.Empty;

            if (File.Exists(fileName))
            {
                await Task.Run(() =>
                {
                    File.Delete(fileName);
                });
            }

            return null;
        }
    }
}
