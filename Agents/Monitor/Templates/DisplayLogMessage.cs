using Technologai;

internal class DisplayLogMessage : Template
{   
    public event EventHandler<string>? LogMessage;

    public DisplayLogMessage()
    {
        Id = "core_display_log_message";
        Description = "Display a message on the output log screen.";        
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override Task<Data?> Process(InformationAdapter information)
    {
        LogMessage?.Invoke(information.CreatorId, information?.InputText ?? string.Empty);
        return Task.FromResult((Data?)null);
    }
}