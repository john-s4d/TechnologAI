using Technologai;

public class GetUserInput : Neuron
{
    public GetUserInput()
    {
        Id = "get_user_input";
        Description = "Receive a text input from the user.";
        //DefaultState = ProcessState.EXECUTE;
    }
    
    public override async Task<object?> Spike(InformationAdapter information)
    {        
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }
}
