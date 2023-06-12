using Technologai;

public class ShowUserOutput : Process
{   
    internal event Action<string>? OutputMessage;

    public new string Name { get; set; } = "Show User Output";
    public new string Description { get; set; } = "Provide the user with information.";
    public new string SampleJsonIn { get; set; } = "{\"message\":\"string\"}";

    public ProcessState Assess(InformationAdapter information)
    {
        /*
        Assessment assessment = new Assessment();

        assessment.Data.Add("message", information.Input ?? string.Empty);
        assessment.Result = AssessmentResult.EXECUTE;

        return Task.FromResult(assessment);
        */
        return this.State;
    }

    public Task<Dictionary<string, object>> Execute(Dictionary<string,object> data)
    {
        OutputMessage?.Invoke((string)data["message"]);

        return Task.FromResult(data);
    }
}
