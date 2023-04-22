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

    public Task<Assessment> Assess(InformationAdapter information)
    {
        information.Assessment.Summary = information.Input ?? string.Empty;
        information.Assessment.Result = AssessmentResult.EXECUTE;

        return Task.FromResult(information.Assessment);
    }

    public Task<string> Execute(Assessment assessment)
    {
        OutputMessage?.Invoke(assessment.Summary);

        return Task.FromResult(string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
}
