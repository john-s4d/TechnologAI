using Technologai;

public interface IProcess
{   
    string? Id { get; }
    string? Description { get; }
    string[]? InputKeys { get; } // TODO: Case Sensitivity
    string[]? OutputKeys { get; } // TODO: Case Sensitivity
    string? WorkerId { get; internal set; }
    ProcessState DefaultState { get; }
    Task<ProcessState> Assess(InformationAdapter information);
    Task<object?> Execute(InformationAdapter information);
    Task<List<InformationAdapter>?> Spawn(InformationAdapter information);
}