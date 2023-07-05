using System.Text.Json;
using System.Text.Json.Serialization;

namespace Technologai
{
    public enum InformationState
    {
        DRAFT = 0,
        OPEN = 1,
        CLOSED = 2
    }

    public class Information : IComparable<Information>
    {
        public string Id { get; private set; }
        public string CreatorId { get; private set; }
        public string WorkerId { get; set; }
        public string ProcessId { get; internal set; }
        public InformationState InformationState { get; set; }        
        //public ProcessState ProcessState { get; set; }

        public Data Input { get; set; } 
        public Data Output { get; set; }

        /*
        private string? _inputText;
        private string? _outputText;

        private Dictionary<string, string>? _inputData;
        private Dictionary<string, string>? _outputData;
        */
        public string? InputText
        {
            get
            {
                return Input.Structured != null ? JsonSerializer.Serialize(Input.Structured) : Convert.ToString(Input.Unstructured); // TODO: Cache
            }
            set
            {
                Input.Unstructured = Input.Unstructured == null ? value : throw new InvalidOperationException("Input is already set");
            }
        }

        public string? OutputText
        {
            get
            {
                return Output.Structured != null ? JsonSerializer.Serialize(Output.Structured) : Convert.ToString(Output.Unstructured); // TODO: Cache
            }
            set
            {
                Output.Unstructured = Output.Unstructured == null ? value : throw new InvalidOperationException("Input is already set");
            }
        }

        [JsonIgnore]
        public Dictionary<string, string>? InputData
        {
            get => Input.Structured;
            set => Input.Structured = value;
        }

        [JsonIgnore]
        public Dictionary<string, string>? OutputData
        {
            get => Output.Structured; 
            set => Output.Structured = value;
        }

        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(string id, string creatorId, string workerId, string neuronId, InformationState informationState, string? inputText = null, string? outputText = null)
        {
            Id = id;
            CreatorId = creatorId;
            WorkerId = workerId;
            ProcessId = neuronId;
            InformationState = informationState;
            //ProcessState = processState;
            InputText = inputText;
            OutputText = outputText;
        }

        public static Information Create(string creatorId, string processId, string? input = null)
        {
            return new Information(
                InformationId.Create(creatorId),
                creatorId,
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
