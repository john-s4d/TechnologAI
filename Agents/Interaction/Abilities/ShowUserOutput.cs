using Technologai;

public class ShowUserOutput : IAbility
{   
    internal event Action<string>? OutputMessage;

    public string Id { get; set; } = "show_user_output";
    public string? Description { get; set; } = "Provide the user with information.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information, Assessment assessment)
    {
        assessment.Result = AssessmentResult.EXECUTE;
        return Task.FromResult(assessment);
    }

    public Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        OutputMessage?.Invoke(information.Input ?? string.Empty);
        return Task.FromResult(information.Input ?? string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(new List<Information>());
    }
}
