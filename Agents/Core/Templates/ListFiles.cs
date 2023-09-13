namespace Technologai.Templates
{
    /// <summary>
    /// List files from a local directory.
    /// </summary>
    internal class ListFiles : Template
    {
        public ListFiles()
        {
            Id = "list_files";
            Description = "List file from the local directory.";
            InputKeys = new string[] { "directory", "includeSubDirectories", "fileExtension" };
            OutputKeys = new string[] { "files" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            string directory = ((string)information.Input.Structured["directory"]).Trim();
            try
            {
                if (!Directory.Exists(directory))
                {
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Directory doesn't exists'" } }));
                }

                var files = await Task.Run(() =>
                {
                    SearchOption searchOption = SearchOption.TopDirectoryOnly;
                    var includeSubDirectories = information.Input.Structured["includeSubDirectories"];
                    if (includeSubDirectories != null && Convert.ToBoolean(includeSubDirectories) == true)
                    {
                        searchOption = SearchOption.AllDirectories;
                    }

                    var fileExtension = information.Input.Structured["fileExtension"];
                    var fileExt = "*";
                    if (fileExtension != null && !string.IsNullOrEmpty((string)fileExtension))
                    {
                        fileExt += ((string)fileExtension).Trim();
                    }
                    var files = Directory.GetFiles(directory, fileExt, searchOption);

                    return files;
                });
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "files", files.ToString() } }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "error", $"Error reading directory '{directory}': {ex.Message}" } }));
            }
        }
    }
}