using Technologai;

public class GetUserInput : IAbility
{
    public string Id { get; set; } = "get_user_input";
    public string? Description { get; set; } = "Receive a response from the user.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information)
    {
        information.Assessment.Result = AssessmentResult.EXECUTE;

        return Task.FromResult(information.Assessment);
    }

    public async Task<string> Execute(Assessment assessment)
    {
        //Console.WriteLine(assessment.Summary);

        var value = await Task.Run(() =>
        {
            return Console.ReadLine() ?? string.Empty;
        });

        if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(Utils.GenerateNewIdString(32));
        }

        return value;
    }

    public Task<List<Information>> Spawn(Assessment assessment)
    {   
        return Task.FromResult(new List<Information>());
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        throw new NotImplementedException();
    }
}
