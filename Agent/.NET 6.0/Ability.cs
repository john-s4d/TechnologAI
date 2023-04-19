using Technologai;

public class Ability : IAbility
{
    public string Id { get; set; }
    public string? Description { get; set; }
    public string? Prompt { get; set; }
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }    

    public Ability(string id)
    {
        Id = id;
    }

    public virtual Task<Assessment> Assess(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(assessment);
    }

    public virtual Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(string.Empty);
    }

    public virtual Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(new List<Information>());
    }
    
}
