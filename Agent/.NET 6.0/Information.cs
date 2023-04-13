using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Technologai
{
    public enum InformationState
    {
        DRAFT,
        OPEN,
        CLOSED
    }

    public class Information
    {
        public string ContextId { get; private set; }
        public string CreatorId { get; private set; }
        public InformationState State { get; internal set; }
        public string? OwnerId { get; internal set; }
        public string? Input { get; internal set; }
        public string? SchemaIn { get; internal set; }
        public string? Output { get; internal set; }
        public string? SchemaOut { get; internal set; }
        public string? AbilityName { get; internal set; }

        // TODO History, Signatures, ReadOnly fields ?        

        private Information() { }

        [JsonConstructor]
        public Information(string contextId, string creatorId, InformationState state, 
            string? ownerId = null, string? input = null, string? schemaIn = null, 
            string? output = null, string? schemaOut = null, string? abilityName = null)
        {
            ContextId = contextId;
            CreatorId = creatorId;
            State = state;
            OwnerId = ownerId;
            Input = input;            
            SchemaIn = schemaIn;
            Output = output;
            SchemaOut = schemaOut;
            AbilityName = abilityName;                
        }

        public Information(string creatorId)
            : this(creatorId, null) { }

        public Information(string creatorId, string? input = null)
        {
            Input = input;
            State = InformationState.DRAFT;
            ContextId = Technologai.ContextId.Create(creatorId);
            CreatorId = creatorId;
        }

        public static Information? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Information>(json);
        }

        public string ToJson()
        {   
            return JsonSerializer.Serialize(this);
        }
    }
}
