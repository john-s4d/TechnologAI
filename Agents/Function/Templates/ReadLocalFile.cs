using Microsoft.VisualBasic;

namespace Technologai.Templates
{
    internal class ReadLocalFile : Template
    {
        public ReadLocalFile()
        {
            Id = "read_local_file";
            Description = "Read a text file on the local filesystem.";
            InputKeys = new string[] { "filename" };
            OutputKeys = new string[] { "contents" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            var filenameObj = information.Input.Structured?["filename"];
            if (information.Input == null || filenameObj != null || !(filenameObj is string filename))
            {
                return Data.Create("error", $"Invalid or missing filename in input data.");
            }

            try
            {
                using StreamReader reader = new StreamReader(filename);
                var contents = await reader.ReadToEndAsync();
                return Data.Create(contents);
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
            }
        }
    }
}