using System.Text.Json.Serialization;

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
        public Process() { }

        public string? Id { get; set; }
        public string? Description { get; set; }
        public string[]? InputKeys { get; set; }
        public string[]? OutputKeys { get; set; }
        public string? MemberId { get; set; }
        [JsonIgnore]
        public ProcessState DefaultState { get; set; } = ProcessState.ASSESS;

        // NOTE: We don't expect these methods to set the object properties (input,output,process). This way they can be inspected by the calling code before the change is committed.
        public virtual Task<ProcessState> Assess(InformationAdapter information) => Task.FromResult(DefaultState);
        public virtual Task<object?> Execute(InformationAdapter information) => Task.FromResult((object?)null);
        public virtual Task<List<InformationAdapter>?> Spawn(InformationAdapter information) => Task.FromResult((List<InformationAdapter>?)null);
    }
}