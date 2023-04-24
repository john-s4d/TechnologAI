using System.Text.Json;
using System.Text.Json.Serialization;
using QuikGraph;

namespace Technologai
{
    public enum InformationState
    {
        DRAFT,
        OPEN,
        CLOSED
    }

    public class Information : IComparable<Information>, IEdge<string?>
    {           
        public string Id { get; private set; }
        public string CreatorId { get; private set; }
        public string? CompletorId { get; private set; }
        public InformationState State { get; internal set; }
        public string? Input { get; internal set; }
        public string? Output { get; internal set; }
        public string AbilityId { get; internal set; }
        public string? Source => Input;
        public string? Target => Output;

        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(
            string id,
            string creatorId,
            string abilityId,
            InformationState state,
            string? input = null,
            string? output = null
            )
        {
            Id = id;
            CreatorId = creatorId;
            AbilityId = abilityId;
            State = state;
            Input = input;
            Output = output;
        }

        public static Information Create(string creatorId, string abilityId, string? input = null)
        {
            return new Information(
                Technologai.ContextId.Create(creatorId),
                creatorId,
                abilityId,
                InformationState.DRAFT,
                input,
                null
                );
        }

        public static Information? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Information>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public int CompareTo(Information? other)
        {
            return object.ReferenceEquals(other, null) ? 1 : ((ContextId)Id).CompareTo((ContextId)other.Id);
        }
    }
}
