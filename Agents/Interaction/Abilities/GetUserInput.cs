using Technologai;

public class GetUserInput : IAbility
{
    public string Id { get; set; } = "get_user_input";
    public string Description { get; set; } = "Receive a response from the user.";
    public string SampleJsonIn { get; set; } = string.Empty;
    public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";
    public string? MemberId { get; set; }

    public Task<Assessment> Assess(InformationAdapter information)
    {
        information.Assessment.Result = AssessmentResult.EXECUTE;

        return Task.FromResult(information.Assessment);
    }

    public async Task<Dictionary<string, object>> Execute(Dictionary<string,object> data)
    {
        var value = await Task.Run(() =>
        {
            return Console.ReadLine() ?? string.Empty;
        });

        /*
        if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(Utils.GenerateNewIdString(32));
        }*/

        return new Dictionary<string, object> { { "output", value } };
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        return Task.FromResult(new List<Information>());
    }
}
