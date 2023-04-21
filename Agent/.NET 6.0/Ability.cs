using QuikGraph;
using Technologai;

public class Ability<> : IAbility, IEdge<Ability>
{
    public string Id { get; set; }
    public string? Description { get; set; }
    public string? Prompt { get; set; }
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }

    public string Source => throw new NotImplementedException();

    public string Target => throw new NotImplementedException();

    public Ability(string id)
    {
        Id = id;
    }

    public virtual Task<AssessmentResult> Assess(InformationAdapter information)
    {
        return Task.FromResult(AssessmentResult.EXECUTE);
    }

    public virtual Task<string> Execute(InformationAdapter information)
    {
        return Task.FromResult(string.Empty);
    }

    public virtual Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
    
}
