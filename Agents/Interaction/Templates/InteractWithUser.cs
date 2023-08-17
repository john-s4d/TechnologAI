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
        var showUserOutput = await information.Spawn("show_output_to_user", information.Input);
        await showUserOutput.PublishAndWait();

        var getUserInput = await information.Spawn("get_input_from_user");
        var userInput = await getUserInput.PublishAndWait();

#if DEBUG

        if (userInput?.Raw?.StartsWith("DEBUG:") ?? false)
        {
            var debugTemplate = await information.Spawn("debug", userInput);
            await debugTemplate.Publish(PublishCallback);
            return null;
        }
#endif

        var bestTemplate = await information.Spawn("get_best_template", userInput).Result.PublishAndWait();        

        await information.Spawn(bestTemplate?.Structured?["Id"] ?? "input_to_output", userInput).Result.Publish(PublishCallback);        

        return new Data();

    }

    private void PublishCallback(InformationAdapter information)
    {
        var showUserOutput = information.Spawn("show_output_to_user", information.Output).Result;
        _ = showUserOutput.Publish();
    }
}
