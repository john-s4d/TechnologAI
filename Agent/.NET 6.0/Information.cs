using System.Management;
using System.Reflection.Metadata.Ecma335;
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

    public class Information : IComparable<Information>
    {
        public string Id { get; private set; }
        public string CreatorId { get; private set; }
        public string OwnerId { get; internal set; }
        public InformationState State { get; internal set; }        
        public string? Input { get; internal set; }
        public string? SampleJsonIn { get; internal set; }
        public string? Output { get; internal set; }
        public string? SampleJsonOut { get; internal set; }
        public string? Signature { get; internal set; } // parentContextId + contextId + memberId + parentHash + output
        public string AbilityName { get; internal set; }
        

        // TODO History, Signatures, ReadOnly fields ?        
        
        [JsonConstructor]        
        public Information(string id, string creatorId, string abilityName, InformationState state, 
            string ownerId, string? input = null, string? sampleJsonIn = null, 
            string? output = null, string? sampleJsonOut = null, string? signature = null)
        {
            Id = id;
            CreatorId = creatorId;
            State = state;
            OwnerId = ownerId;
            Input = input;
            SampleJsonIn = sampleJsonIn;
            Output = output;
            SampleJsonOut = sampleJsonOut;
            AbilityName = abilityName;
            Signature = signature;
        }

        public Information(string creatorId, string abilityName, string? input = null)
        {
            AbilityName = abilityName;
            OwnerId = creatorId;
            CreatorId = creatorId;
            Input = input;
            State = InformationState.DRAFT;
            Id = Technologai.ContextId.Create(creatorId);
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
