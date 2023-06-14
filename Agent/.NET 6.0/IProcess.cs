using Technologai;

public interface IProcess
{   
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string[]? ParametersIn { get; set; }
    public string[]? ParametersOut { get; set; }
    public string? WorkerId { get; set; }    
    public ProcessState State { get; set; }
    public Task<ProcessState> Assess(InformationAdapter information);
    public Task<object?> Execute(InformationAdapter information);
    public Task<List<InformationAdapter>?> Spawn(InformationAdapter information);
}