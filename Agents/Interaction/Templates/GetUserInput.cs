using Technologai;

public class GetUserInput : Template
{
    public GetUserInput()
    {
        Id = "get_user_input";
        Description = "Receive a text input from the user.";
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override async Task<Data?> Process(InformationAdapter information)
    {        
        return await Task.Run(() =>
        {
            var output = new Data(Console.ReadLine() ?? string.Empty);
            return output;
        });
    }
}
