using Technologai;

public class InteractWithUser : Process
{
    public InteractWithUser()
    {
        Id = "interact_with_user";
        Description = "Provide the user with information and receive a response from the user.";        
    }
  
    public override Task<ProcessState> Assess(InformationAdapter information)
    {
        return Task.FromResult(information.InputText != null ? ProcessState.EXECUTE : ProcessState.ASSESS);
    }

    public override async Task<List<Information>?> Spawn(InformationAdapter information)
    {
        List<Information> result = new List<Information>
        {
            await information.Spawn("show_user_output", information.InputText),
            await information.Spawn("get_user_input")
        };

        return result;
    }
}
