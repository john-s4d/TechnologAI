using Technologai;

public class InteractWithUser : Template
{
    public InteractWithUser()
    {
        Id = "interact_with_user";
        Description = "Provide the user with information and receive a response from the user.";
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override async Task<Data?> Process(InformationAdapter information)
    {
        var showUserOutput = await information.Spawn("show_user_output", information.Input);
        await showUserOutput.PublishAndWait();

        var getUserInput = await information.Spawn("get_user_input");
        var userInput = await getUserInput.PublishAndWait();
        
        var getBestTemplate = await information.Spawn("get_best_template", userInput?.Raw); // TODO: Span should take Data object instead of string parameter
        var bestTemplate = await getBestTemplate.PublishAndWait();

        if (bestTemplate?.Structured?["Id"] == "echo_user_input")
        {
            return userInput;
        }

        var chosenTemplate = await information.Spawn(bestTemplate?.Structured?["Id"] ?? "echo_user_input", userInput?.Raw);
        return await chosenTemplate.PublishAndWait();        
    }   
}
