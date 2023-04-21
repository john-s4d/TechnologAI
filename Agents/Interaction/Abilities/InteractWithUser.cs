using Technologai;

public class InteractWithUser : IAbility
{
    public string Id { get; set; } = "interact_with_user";
    public string? Description { get; set; } = "Provide the user with information and receive a response from the user.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<AssessmentResult> Assess(InformationAdapter information)
    {
        // TODO: Simplify the assessment.
        // Assess looks at the input && context, makes or updates assessment, and then decides if we can execute or spawn.

        
        //information.Assessment = information.Context.ContainsEdge()
        
        var result = string.IsNullOrEmpty(information.Assessment) ? AssessmentResult.SPAWN : AssessmentResult.EXECUTE;

        return Task.FromResult(result);
    }

    public Task<string> Execute(InformationAdapter information)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        List<Information> result = new List<Information>();

        var showUserOutput = information.GetSpawn("show_user_output", "Hello");
        var getUserInput = showUserOutput.GetSpawn("get_user_input");        
                
        result.Add(showUserOutput);
        result.Add(getUserInput);

        return Task.FromResult(result);
        
    }
}
