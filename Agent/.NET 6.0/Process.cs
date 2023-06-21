using System.Text.Json.Serialization;

namespace Technologai
{
    public enum ProcessState
    {
        ASSESS = 0,
        EXECUTE = 1,
        SPAWN = 2,
        COMPLETE = 3
    }
    public class Process : IProcess
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string[]? InputKeys { get; set; }
        public string[]? OutputKeys { get; set; }
        public string? MemberId { get; set; }
        [JsonIgnore]
        public ProcessState? DefaultState { get; set; } = null;

        // NOTE: We don't expect these methods to set the object properties (input,output,process). This way they can be inspected by the calling code before the change is committed.
        public virtual Task<ProcessState> Assess(InformationAdapter information) => Task.FromResult(DefaultState ?? ProcessState.ASSESS);
        public virtual Task<object?> Execute(InformationAdapter information) => Task.FromResult((object?)null);
        public virtual Task<List<Information>?> Spawn(InformationAdapter information) => Task.FromResult((List<Information>?)null);
    }
}