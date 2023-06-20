using Technologai;

public class ShowUserOutput : Process
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput()
    {
        Id = "show_user_output";
        Description = "Display a message on the output log screen.";

    }   
    
    public new ProcessState Assess(InformationAdapter information)
    {   
        return information.InputText != null ? ProcessState.EXECUTE : DefaultState;
    }

    public new void Execute(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information.InputText ?? string.Empty);
    }
}
