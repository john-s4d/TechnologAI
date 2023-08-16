using Technologai;

public class GetInputFromUser : Template
{
    public GetInputFromUser()
    {
        Id = "get_input_from_user";
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
