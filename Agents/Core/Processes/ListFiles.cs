namespace Technologai
{
    /// <summary>
    /// List files from a local directory.
    /// </summary>
    internal class ListFiles : Process
    {
        public string Description { get; } = "List file from the local directory.";
        public string SampleJsonIn { get; } = "{\"directory\":\"string\",\"includeSubDirectories\":\"bool\",\"fileExtension\":\"string\"}"; // optional: includeSubDirectories, fileExtension
        public string SampleJsonOut { get; } = "{\"files\":\"string[]\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            string directory = ((string)data["directory"]).Trim();
            try
            {
                if (!Directory.Exists(directory))
                {
                    return new Dictionary<string, object> { { "error", $"Directory doesn't exists'" } };
                }

                var files = await Task.Run(() =>
                {
                    SearchOption searchOption = SearchOption.TopDirectoryOnly;
                    data.TryGetValue("includeSubDirectories", out object includeSubDirectories);
                    if (includeSubDirectories != null && (bool)includeSubDirectories == true)
                    {
                        searchOption = SearchOption.AllDirectories;
                    }

                    data.TryGetValue("fileExtension", out object fileExtension);
                    var fileExt = "*";
                    if (fileExtension != null && !string.IsNullOrEmpty((string)fileExtension))
                    {
                        fileExt += ((string)fileExtension).Trim();
                    }
                    var files = Directory.GetFiles(directory, fileExt, searchOption);

                    return files;
                });
                return new Dictionary<string, object>() { { "files", files } };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error reading directory '{directory}': {ex.Message}" } };
            }
        }
    }
}