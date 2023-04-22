using QuikGraph;
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

    public virtual Task<Assessment> Assess(InformationAdapter information)
    {
        return Task.FromResult(information.Assessment);
    }

    public virtual Task<string> Execute(Assessment assessment)
    {
        return Task.FromResult(assessment.Summary);
    }

    public virtual Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
    
}
