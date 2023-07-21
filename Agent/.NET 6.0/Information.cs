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
        public string NeuronId { get; internal set; }
        public InformationState InformationState { get; set; }        
        public NeuronState NeuronState { get; set; }

        [JsonIgnore]
        public Data Input { get; set; } = new Data();
        [JsonIgnore]
        public Data Output { get; set; } = new Data();

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
        public Information(string id, string creatorId, string workerId, string neuronId, InformationState informationState, NeuronState neuronState, string? inputText = null, string? outputText = null)
        {
            Id = id;
            CreatorId = creatorId;
            WorkerId = workerId;
            NeuronId = neuronId;
            InformationState = informationState;
            NeuronState = neuronState;
            InputText = inputText;
            OutputText = outputText;
        }

        public static Information Create(string creatorId, string neuronId, string? input = null)
        {
            return new Information(
                InformationId.Create(creatorId),
                creatorId,
                creatorId,
                neuronId,
                InformationState.DRAFT,
                NeuronState.RESTING,
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
