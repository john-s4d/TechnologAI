using System.ComponentModel;
using Technologai;

public interface IProcess
{   
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }    
    public Task<Assessment> Assess(InformationAdapter information);    
    public Task<ExecuteResult> Execute(Assessment assessment);
    public Task<List<Information>> Spawn(InformationAdapter information);
}