using Technologai;

public class GetUserInput : Process
{
    public string? Name { get; set; } = "get_user_input";
    public new string Description { get; set; } = "Receive a response from the user.";
    public new string SampleJsonIn { get; set; } = string.Empty;
    public new string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";

    public new ProcessState Assess(InformationAdapter information)
    {
        return ProcessState.EXECUTE;
    }

public async Task<Dictionary<string, object>> Execute(InformationAdapter information)
{
    var value = await Task.Run(() =>
    {
        return Console.ReadLine() ?? string.Empty;
    });



    return new Dictionary<string, object> { { "output", value } };
}

public Task<List<Information>> Spawn(InformationAdapter information)
{
    return Task.FromResult(new List<Information>());
}
}
