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

    public enum InformationStructure
    {
        UNKNOWN,
        TEXT,
        PARAMETERS        
    }

    public class Information : IComparable<Information>
    {
        public string Id { get; private set; }
        public string CreatorId { get; private set; }
        public InformationState State { get; internal set; }
        //public InformationStructure InputStructure { get; internal set; } = InformationStructure.UNKNOWN;
        //public InformationStructure OutputStructure { get; internal set; } = InformationStructure.UNKNOWN;

        private string? _input;
        private string? _output;

        private Dictionary<string,string>? _inputParameters;
        private Dictionary<string, string>? _outputParameters;
        /*
        public void SetInput(string input)
        {
            InputStructure = InformationStructure.TEXT;
            _input = input;
        }

        public void SetInput(Dictionary<string,string> input)
        {
            InputStructure = InformationStructure.PARAMETERS;
            _inputParameters = input;
            _input = JsonSerializer.Serialize(input);
        }*/

        public string? Input
        {
            get
            {
                return InputStructure == InformationStructure.PARAMETERS ? JsonSerializer.Serialize(InputParameters) : _input; // TODO: Cache, JIT
            }
            private set
            {
                _input = InputStructure == InformationStructure.PARAMETERS ? value : throw new InvalidOperationException("Input is only writable when Structure is TEXT");
            }
        }
                
        public string? Output
        {
            get
            {
                return OutputStructure == InformationStructure.PARAMETERS ? JsonSerializer.Serialize(OutputParameters) : _output; // TODO: Cache, JIT
            }
            internal set
            {
                _output = OutputStructure == InformationStructure.PARAMETERS ? value : throw new InvalidOperationException("Output is only writable when Structure is TEXT");
            }
        }

        [JsonIgnore]
        public Dictionary<string, string>? InputParameters { get; internal set; }

        [JsonIgnore]
        public Dictionary<string, string>? OutputParameters { get; internal set; }

        //[JsonIgnore]
        //public Tensor<float>? OutputTensor { get; internal set; }

        //[JsonIgnore]
        //public Tensor<float>? InputTensor { get; internal set; }

        public string ProcessId { get; internal set; }



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
                InformationId.Create(creatorId),
                creatorId,
                processId,
                InformationState.DRAFT,
                input,
                null
                );
        }

        public int CompareTo(Information? other)
        {
            return object.ReferenceEquals(other, null) ? 1 : ((InformationId)Id).CompareTo((InformationId)other.Id);
        }
    }
}
