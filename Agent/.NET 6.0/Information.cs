using System.Text.Json;
using System.Text.Json.Serialization;
//using System.Numerics.Tensors;
//using Tensornet;

namespace Technologai
{
    public enum InformationState
    {
        DRAFT,
        OPEN,
        CLOSED
    }

    public class Information : IComparable<Information>
    {


        public string Id { get; private set; }
        public string CreatorId { get; private set; }
        public InformationState State { get; internal set; }

        // Returns either a string or json
        public string? Input { get; internal set; }

        public string? Output { get; internal set; }

        [JsonIgnore]
        public Dictionary<string, string>? InputParameters { get; internal set; }

        [JsonIgnore]
        public Dictionary<string, string>? OutputParameters { get; internal set; }

        //[JsonIgnore]
        //public Tensor<float>? OutputTensor { get; internal set; }

        //[JsonIgnore]
        //public Tensor<float>? InputTensor { get; internal set; }

        public string ProcessId { get; internal set; }

        private IConvertible? _input;
        private IConvertible? _output;

        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(string id, string creatorId, string processId, InformationState state, string? input = null, string? output = null)
        {
            Id = id;
            CreatorId = creatorId;
            ProcessId = processId;
            State = state;
            Input = input;
            Output = output;
        }

        public static Information Create(string creatorId, string processId, string? input = null)
        {
            return new Information(
                Technologai.ContextId.Create(creatorId),
                creatorId,
                processId,
                InformationState.DRAFT,
                input,
                null
                );
        }

        public int CompareTo(Information? other)
        {
            return object.ReferenceEquals(other, null) ? 1 : ((ContextId)Id).CompareTo((ContextId)other.Id);
        }
    }
}
