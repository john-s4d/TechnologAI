namespace Technologai
{
    internal class AppendToFile : Process
    {
        public AppendToFile()
        {
            // TODO: Case Sensitivity

            Id = "append_to_file";
            Description = "Append text to file in the local filesystem.";
            InputKeys = new string[] { "filename", "content" };
        }

        public static new ProcessState Assess(InformationAdapter information)
        {
            return (information.InputData?["filename"] != null &&
                    information.InputData?["content"] != null) ?
                        ProcessState.EXECUTE :
                        ProcessState.ASSESS;
        }

        public static new async Task Execute(InformationAdapter information)
        {
            using (var writer = new StreamWriter(information?.InputData?["filename"] 
                       ?? throw new ArgumentNullException("filename"), true))
            {
                await writer.WriteAsync(information?.InputData?["content"]);
            }
        }
    }
}