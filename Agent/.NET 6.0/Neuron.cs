using System.Text.Json.Serialization;

namespace Technologai
{
    public class Neuron : INeuron
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string[]? InputKeys { get; set; }
        public string[]? OutputKeys { get; set; }
        public string? MemberId { get; set; }

        // NOTE: We don't expect these methods to set the object properties (input,output,process). This way they can be inspected by the calling code before the change is committed.
        public virtual Task<bool> Assess(InformationAdapter information) => Task.FromResult(false);
        public virtual Task<object?> Spike(InformationAdapter information) => Task.FromResult((object?)null);
        public virtual Task Recover(Context context) => Task.CompletedTask;
        public virtual Task Rest(Context context) => Task.CompletedTask;
    }
}