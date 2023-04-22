using Technologai;

public interface IAbility : IExecute
{
    public string Id { get; set; }    
    public string? Prompt { get; set; }    
    public string? MemberId { get; set; }

    public Task<Assessment> Assess(InformationAdapter information);
    public Task<List<Information>> Spawn(InformationAdapter information);
}