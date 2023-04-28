using Technologai;

public class ShowUserOutput : IProcess
{   
    internal event Action<string>? OutputMessage;

    public string Id { get; set; } = "show_user_output";
    public string Description { get; set; } = "Provide the user with information.";
    public string SampleJsonIn { get; set; } = "{\"message\":\"string\"}";
    public string SampleJsonOut { get; set; } = string.Empty;
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information)
    {
        information.Assessment.Data.Add("message", information.Input ?? string.Empty);
        information.Assessment.Result = AssessmentResult.EXECUTE;

        return Task.FromResult(information.Assessment);
    }

    public Task<Dictionary<string, object>> Execute(Dictionary<string,object> data)
    {
        OutputMessage?.Invoke((string)data["message"]);

        return Task.FromResult(data);
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
}
