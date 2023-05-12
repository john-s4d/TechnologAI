using Technologai;

public interface IProcess : IExecute
{
    public string Id { get; set; }        
    public string? MemberId { get; set; }


    public Task<Assessment> Assess(InformationAdapter information);
    public Task<List<Information>> Spawn(InformationAdapter information);    
}