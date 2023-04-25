using System.Xml;
using Technologai;

public class InteractWithUser : IAbility
{
    public string Id { get; set; } = "interact_with_user";
    public string Description { get; set; } = "Provide the user with information and receive a response from the user.";
    public string SampleJsonIn { get; set; } = "{\"input\":\"string\"}";
    public string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";
    public string? MemberId { get; set; }
    public string? Prompt { get; set; }

    public Task<Assessment> Assess(InformationAdapter information)
    {
        foreach(var item in information.Context.GetForward(information.ContextId)) { 
            if (item.AbilityId == "get_user_input")
            {
                information.Assessment.Data.Add("output", item.Output ?? string.Empty);
                break;
            }
        }

        information.Assessment.Result = information.Assessment.Data.ContainsKey("output") ? AssessmentResult.EXECUTE : AssessmentResult.SPAWN;

        return Task.FromResult(information.Assessment);
    }

    public Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
    {
        //data["output"] = data["input"];

        return Task.FromResult(data);
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        List<Information> result = new List<Information>();
        
        result.Add(information.GetSpawn("show_user_output", information.Input));
        result.Add(information.GetSpawn("get_user_input"));

        return Task.FromResult(result);
    }
}
