namespace Technologai
{
    public enum ProcessState
    {
        ASSESS,        
        EXECUTE,
        SPAWN
    }

    public class Process : IProcess
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string[]? ParametersIn { get; set; }
        public string[]? ParametersOut { get; set; }        
        public string? WorkerId { get; set; }
        public ProcessState State { get; set; } = ProcessState.ASSESS;

        public virtual ProcessState Assess(in InformationAdapter information)
        {
            // Assess the information and set this.State property.

            return this.State;
        }

        public virtual string? Execute(in InformationAdapter information)
        {
            // Execute the process and return the output            

            return null;
        }
        /*
        public virtual List<Information> Spawn(in InformationAdapter information)
        {
            // Spawn new information and return it.

            return new List<Information>();
        }*/

        List<InformationAdapter> IProcess.Spawn(in InformationAdapter information)
        {
            return new List<InformationAdapter>();
        }
    }
}