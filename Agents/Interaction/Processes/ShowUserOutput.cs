using Technologai;

public class ShowUserOutput : Process
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput()
    {
        Id = "show_user_output";
        Description = "Display a message on the output log screen.";
    }   
    
    public override Task<ProcessState> Assess(InformationAdapter information)
    {   
        return information.InputText != null ? Task.FromResult(ProcessState.EXECUTE) : Task.FromResult(ProcessState.ASSESS);
    }

    public override Task<object?> Execute(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information.InputText ?? string.Empty);
        return Task.FromResult((object?)null);
    }
}
