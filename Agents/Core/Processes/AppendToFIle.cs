namespace Technologai
{
    internal class AppendToFile : Process
    {
        public AppendToFile()
        {
            Name = "Append to File";
            Description = "Append text to file on the local filesystem.";
            ParametersIn = new string[] { "fileName", "content" };
        }

        public new ProcessState Assess(InformationAdapter information)
        {

           return (information.Input["fileName"] != null && information.Input["content"] != null) ?
                ProcessState.EXECUTE : 
                    ProcessState.ASSESS;*/
           return ProcessState.EXECUTE;
        }

        public new string? Execute(in InformationAdapter information)
        {
            /*
            using StreamWriter writer = new((string)information.Input["fileName"], true);
            writer.WriteAsync((string)information["content"]).Wait();            
            return null;*/
            return null;
        }
    }
}