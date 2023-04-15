using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public string OwnerId { get; internal set; }
        public InformationState State { get; internal set; }        
        public string? Input { get; internal set; }
        public string? SampleJsonIn { get; internal set; }
        public string? Output { get; internal set; }
        public string? SampleJsonOut { get; internal set; }
        public string AbilityName { get; internal set; }

        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(string contextId, string creatorId, InformationState state, 
            string ownerId, string? input = null, string? sampleJsonIn = null, 
            string? output = null, string? sampleJsonOut = null, string? abilityName = null)
        {
            ContextId = contextId;
            CreatorId = creatorId;
            State = state;
            OwnerId = ownerId;
            Input = input;
            SampleJsonIn = sampleJsonIn;
            Output = output;
            SampleJsonOut = sampleJsonOut;
            AbilityName = abilityName;                
        }

        public Information(string creatorId, string? input = null)
        {
            OwnerId = creatorId;
            CreatorId = creatorId;
            Input = input;
            State = InformationState.DRAFT;
            ContextId = Technologai.ContextId.Create(creatorId);
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
