namespace Technologai
{
    /// <summary>
    /// Delete a file from the local filesystem.
    /// </summary>
    public class DeleteFile : Process
    {
        public string Description { get; } = "Delete a text file on the local filesystem";
        public string SampleJsonIn { get; } = "{\"filename\":\"string\"}";
        public string SampleJsonOut { get; } = "{\"contents\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            string fileName = ((string)data["fileName"]).Trim();
            try
            {
                if (!File.Exists(fileName))
                {
                    return new Dictionary<string, object> { { "error", $"Error file name is not found'" } };
                }
                await Task.Run(() =>
                {
                    File.Delete(fileName);
                });

                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "error", $"Error writting content to file '{fileName}': {ex.Message}" } };
            }

        }
    }
}