using Technologai;

public class InteractWithUser : Neuron
{
    public InteractWithUser()
    {
        Id = "interact_with_user";
        Description = "Provide the user with information and receive a response from the user.";
    }

    public override async Task<bool> Assess(InformationAdapter information)
    {
        return true;
    }

    public override async Task<Data?> Spike(InformationAdapter information)
    {        
        //information.Spawn().Publish()
        await (await information.Spawn("show_user_output", information.InputText)).Publish(ShowUserOutputCallback);        
        return null;        
    }

    private async void ShowUserOutputCallback(InformationAdapter information)
    {
        await(await information.Spawn("get_user_input")).Publish();
    }
}
