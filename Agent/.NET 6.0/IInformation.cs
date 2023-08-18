using System.Text.Json.Serialization;

namespace Technologai
{
    public enum InformationState
    {
        DRAFT = 0,
        OPEN = 1,
        CLOSED = 2
    }

    public interface IInformation : IComparable<Information>
    {
        string Id { get; }
        string CreatorId { get; }
        string? WorkerId { get; set; }
        string TemplateId { get; }
        InformationState InformationState { get; }
        TemplateState TemplateState { get; }
        Data? Input { get; }
        Data? Output { get;}
    }
}
