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

    public virtual Task<Dictionary<string, object>> Execute(Dictionary<string,object> data)
    {
        return Task.FromResult(data);
    }

    public virtual Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
    
}
