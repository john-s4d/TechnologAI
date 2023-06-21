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
        public InformationState InformationState { get; set; }        
        public ProcessState ProcessState { get; set; }

        private string? _inputText;
        private string? _outputText;

        private Dictionary<string, string>? _inputData;
        private Dictionary<string, string>? _outputData;

        public string? InputText
        {
            get
            {
                return _inputData != null ? JsonSerializer.Serialize(_inputData) : _inputText; // TODO: Cache
            }
            set
            {
                _inputText = _inputText == null ? value : throw new InvalidOperationException("Input is already set");
            }
        }

        public string? OutputText
        {
            get
            {
                return _outputData != null ? JsonSerializer.Serialize(_outputData) : _outputText; // TODO: Cache
            }
            set
            {
                _outputText = _outputText == null ? value : throw new InvalidOperationException("Input is already set");
            }
        }

        [JsonIgnore]
        public Dictionary<string, string>? InputData
        {
            get => _inputData;
            set => _inputData = value;
        }

        [JsonIgnore]
        public Dictionary<string, string>? OutputData
        {
            get => _outputData; 
            set => _outputData = value;
        }

        public string ProcessId { get; internal set; }


        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(string id, string creatorId, string workerId, string processId, InformationState informationState, ProcessState processState, string? inputText = null, string? outputText = null)
        {
            Id = id;
            CreatorId = creatorId;
            WorkerId = workerId;
            ProcessId = processId;
            InformationState = informationState;
            ProcessState = processState;
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
                ProcessState.ASSESS,
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
