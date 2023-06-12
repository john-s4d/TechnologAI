using System.Xml;
using Technologai;

public class InteractWithUser : Process
{
    public new string Name { get; set; } = "Interact with user.";
    public new string Description { get; set; } = "Provide the user with information and receive a response from the user.";
    public new string SampleJsonIn { get; set; } = "{\"input\":\"string\"}";
    public new string SampleJsonOut { get; set; } = "{\"output\":\"string\"}";    

    public ProcessState Assess(InformationAdapter information)
    {
        /*
        foreach(var item in information.Context.GetForward(information.ContextId)) { 
            if (item.ProcessId == "get_user_input")
            {
                assessment.Data.Add("output", item.Output ?? string.Empty);
                break;
            }
        }

        assessment.Result = assessment.Data.ContainsKey("output") ? AssessmentResult.EXECUTE : AssessmentResult.SPAWN;
        

        return Task.FromResult(assessment);
        */

        return ProcessState.EXECUTE;
    }

    public Task<List<Information>> Spawn(InformationAdapter information)
    {
        List<Information> result = new List<Information>();
        
        result.Add(information.GetSpawn("show_user_output", information.Input));
        result.Add(information.GetSpawn("get_user_input"));

        return Task.FromResult(result);
    }
}
