using Technologai;

public class InteractWithUser : IAbility
{
    public string Id { get; set; } = "interact_with_user";
    public string? Description { get; set; } = "Provide the user with information and receive a response from the user.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<AssessmentResult> Assess(InformationAdapter information, Assessment assessment)
    {
        // TODO: Simplify the assessment.

        // Assess looks at the input && context, makes or updates assessment, and then decides if we can execute or spawn.

        assessment.Content = information.Context.ForwardContext?[0].Input;

        var result = string.IsNullOrEmpty(assessment.Content) ? AssessmentResult.SPAWN : AssessmentResult.EXECUTE;

        return Task.FromResult(result);
    }

    public Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(assessment.Content ?? string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        List<Information> result = new List<Information>();

        var showUserOutput = information.GetSpawn("show_user_output", "Hello");
        var getUserInput = showUserOutput.GetSpawn("get_user_input");        
                
        result.Add(showUserOutput);
        result.Add(getUserInput);

        return Task.FromResult(result);
        
    }
}
