using Technologai;

public class GetUserInput : Process
{
    public GetUserInput() : base()
    {
       
    }

    public GetUserInput(ref ProcessState defaultState)
    {
        Id = "get_user_input";
        Description = "Receive a text input from the user.";
        defaultState = ProcessState.EXECUTE;
    }
    
    public new async Task<string?> Execute(InformationAdapter information)
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }
}
