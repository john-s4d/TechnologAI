using Technologai;

public class ShowUserOutput : Neuron
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput(Action<string> outputMessageCallback)
    {
        Id = "show_user_output";
        Description = "Display a message on the output log screen.";
        OutputMessage += outputMessageCallback;
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override Task<Data?> Spike(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information.InputText ?? string.Empty);
        return Task.FromResult((Data?)null);
    }
}
