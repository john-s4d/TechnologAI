using Technologai;

public class ShowUserOutput : Template
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput(Action<string> outputMessageCallback)
    {
        Id = "show_user_output";
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
