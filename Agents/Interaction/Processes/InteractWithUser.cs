using Technologai;

public class InteractWithUser : Neuron
{
    public InteractWithUser()
    {
        Id = "interact_with_user";
        Description = "Provide the user with information and receive a response from the user.";
        //DefaultState = ProcessState.SPAWN;
    }

    public override async Task<object?> Spike(InformationAdapter information)
    {
        await (await information.Spawn("show_user_output", information.InputText)).Publish();
        await (await information.Spawn("get_user_input")).Publish();
        return null;        
    }
}
