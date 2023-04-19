using Technologai;

public interface IAbility
{
    public string Id { get; set; }
    public string? Description { get; set; }
    public string? Prompt { get; set; }
    public string? SampleJsonIn { get;  set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }

    public Task<Assessment> Assess(InformationAdapter information, Assessment assessment);

    public Task<string> Execute(InformationAdapter information, Assessment assessment);

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment);
}