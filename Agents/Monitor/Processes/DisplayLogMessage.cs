using Technologai;

internal class DisplayLogMessage : Neuron
{
    internal event Action<string>? LogMessage;

    public DisplayLogMessage()
    {
        Id = "display_log_message";
        Description = "Display a message on the output log screen.";        
    }

    public override Task<Data?> Spike(InformationAdapter information)
    {
        LogMessage?.Invoke(information?.InputText ?? string.Empty);
        return Task.FromResult((Data?)null);
    }
}