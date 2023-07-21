namespace Technologai
{
    public interface INeuron
    {
        string? Id { get; }
        string? Description { get; }
        string[]? InputKeys { get; } // TODO: Case Sensitivity
        string[]? OutputKeys { get; } // TODO: Case Sensitivity
        string? MemberId { get; internal set; }
    }
}