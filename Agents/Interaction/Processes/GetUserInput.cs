using Technologai;

public class GetUserInput : Process
{
    public GetUserInput()
    {
        Id = "get_user_input";
        Description = "Receive a text input from the user.";
        State = ProcessState.EXECUTE;
    }
    
    public new async Task<string?> Execute(InformationAdapter information)
    {
        return await Task.Run(() =>
        {
            return Console.ReadLine();
        });
    }
}
