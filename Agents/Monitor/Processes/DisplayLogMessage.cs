using Technologai;

internal class DisplayLogMessage : Neuron
{
    internal event Action<string>? LogMessage;

    public DisplayLogMessage()
    {
        Id = "display_log_message";
        Description = "Display a message on the output log screen.";
        //DefaultState = ProcessState.EXECUTE;
        //ExecuteStyle = ProcessStyle.ONCE;
    }

    public override Task<object?> Spike(InformationAdapter information)
    {
        LogMessage?.Invoke(information?.InputText ?? string.Empty);
        return Task.FromResult((object?)null);
    }
}