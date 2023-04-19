using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Technologai;

public class GetUserInput : IAbility
{
    public string Id { get; set; } = "get_user_input";
    public string? Description { get; set; } = "Receive a response from the user.";
    public string? SampleJsonIn { get; set; }
    public string? SampleJsonOut { get; set; }
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(assessment);
    }

    public async Task<string> Execute(InformationAdapter information, Assessment assessment)
    {
        Console.WriteLine(information.Input);

        string value = await Task.Run(() =>
        {
            return Console.ReadLine() ?? string.Empty;
        });

        if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(Utils.GenerateNewIdString(32));
        }

        return value;
    }

    public Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
    {
        return Task.FromResult(new List<Information>());
    }
}
