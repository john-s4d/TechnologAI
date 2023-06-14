namespace Technologai
{
    public enum ProcessState
    {
        ASSESS,        
        EXECUTE,
        SPAWN,
        COMPLETE
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

        // NOTE: We don't expect these methods to set the object properties (input,output,process). This way they can be inspected before the change is committed.

        public virtual Task<ProcessState> Assess(InformationAdapter information)
        {
            // Assess the information and return the new or existing state.
            return Task.FromResult(State);
        }

        public virtual Task<object?> Execute(InformationAdapter information)
        {
            // Execute the process and return the output text            
            return Task.FromResult((object?)null);
        }

        public virtual Task<List<InformationAdapter>?> Spawn(InformationAdapter information)
        {
            // Spawn new information as needed
            return Task.FromResult((List<InformationAdapter>?)null);
        }
    }
}