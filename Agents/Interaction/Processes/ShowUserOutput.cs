using Technologai;

public class ShowUserOutput : Neuron
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput()
    {
        Id = "show_user_output";
        Description = "Display a message on the output log screen.";
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override Task<Data?> Spike(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information.InputText ?? string.Empty);
        return Task.FromResult((Data?)null);
    }
}
