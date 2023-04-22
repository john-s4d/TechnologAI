using Technologai;

public interface IExecute
{
    public string? Description { get; set; }
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }

    public Task<string> Execute(Assessment assessment);    
}