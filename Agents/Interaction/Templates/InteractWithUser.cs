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
        var showUserOutput = await information.Spawn("show_user_output", information.InputText);
        await showUserOutput.PublishAndWait();

        var getUserInput = await information.Spawn("get_user_input");
        var userInput = await getUserInput.PublishAndWait();

        if (userInput?.Unstructured == "32bit")
        {
            var create32bit = await information.Spawn("generate_32_bit_string");
            return await create32bit.PublishAndWait();
        }

        return userInput;
    }   
}
