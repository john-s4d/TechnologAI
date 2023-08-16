using Technologai;

public class ShowOutputToUser : Template
{   
    internal event Action<string>? OutputMessage;

    public ShowOutputToUser(Action<string> outputMessageCallback)
    {
        Id = "show_output_to_user";
        Description = "Display a message on the output log screen.";
        OutputMessage += outputMessageCallback;
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override Task<Data?> Process(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information?.Input?.Raw ?? string.Empty);
        return Task.FromResult((Data?)null);
    }
}
