namespace Technologai
{
    /// <summary>
    /// Append to a text file on the local filesystem.
    /// </summary>
    internal class AppendToFile : Process
    {
        public AppendToFile()
        {
            Name = "Append to File";
            Description = "Append text to file on the local filesystem.";
            ParametersIn = new string[] { "fileName", "content" };
        }

        public new ProcessState Assess(in InformationAdapter information)
        {
            return (information["fileName"] != null && information["content"] != null) ?
                ProcessState.EXECUTE : 
                    ProcessState.ASSESS;
        }

        public new string? Execute(in InformationAdapter information)
        {   
            using StreamWriter writer = new((string)information["fileName"], true);
            writer.WriteAsync((string)information["content"]).Wait();            
            return null;
        }
    }
}