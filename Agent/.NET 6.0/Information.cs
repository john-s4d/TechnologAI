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
        public string TemplateId { get; internal set; }
        public InformationState InformationState { get; internal set; }        
        public TemplateState TemplateState { get; internal set; }
                
        public Data? Input { get; internal set; }

        public Data? Output { get; internal set; }

        // TODO History, Signatures, ReadOnly fields ?        

        [JsonConstructor]
        public Information(string id, string creatorId, string workerId, string templateId, InformationState informationState, TemplateState templateState, Data? input = null, Data? output = null)
        {
            Id = id;
            CreatorId = creatorId;
            WorkerId = workerId;
            TemplateId = templateId;
            InformationState = informationState;
            TemplateState = templateState;
            Input = input;
            Output = output;
        }

        public static Information Create(string creatorId, string templateId, Data? input = null)
        {
            return new Information(
                InformationId.Create(creatorId),
                creatorId,
                creatorId,
                templateId,
                InformationState.DRAFT,
                TemplateState.RESTING, 
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
