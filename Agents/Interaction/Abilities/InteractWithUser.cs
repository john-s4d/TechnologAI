using Technologai;

public class InteractWithUser : IAbility
{

    public string Id { get; set; } = "interact_with_user";
    public string? Description { get; set; } = "Provide the user with information and receive a response from the user.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information, Assessment assessment)
    {
        //assessment.ForwardSummary = assessment.ForwardContext?[0].Output;

        if (!string.IsNullOrEmpty(information.Output))
        {
            //forwardContext?[0].Output ?? string.Empty;            
        }

        return Task.FromResult(assessment);
    }

    public Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(information.Input ?? string.Empty);
    }

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        List<Information> result = new List<Information>
        {
            information.Spawn("get_user_input", information.Input),
            information.Spawn("show_user_output", information.Output)
        };

        return Task.FromResult(result);
    }
}
