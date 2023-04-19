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
        
        // TODO: Easily complete the assessment.

        //assessment.ForwardSummary = assessment.ForwardContext?[0].Output;

        if (string.IsNullOrEmpty(information.Output))
        {
            return Task.FromResult(AssessmentResult.SPAWN);
        }
        else
        {
            return Task.FromResult(AssessmentResult.EXECUTE);
        }
    }

    public Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(information.Input ?? string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        List<Information> result = new List<Information>
        {   
            information.GetSpawn("get_user_input", information.Input),
            information.GetSpawn("show_user_output", information.Output)
        };

        return Task.FromResult(result);
    }
}
