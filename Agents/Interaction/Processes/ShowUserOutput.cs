using Technologai;

public class ShowUserOutput : Process
{   
    internal event Action<string>? OutputMessage;

    public ShowUserOutput()
    {
        Name = "Show User Output";
        Description = "Display a message on the output log screen.";
    }   
    
    public new ProcessState Assess(InformationAdapter information)
    {   
        return information.InputText != null ? ProcessState.EXECUTE : this.State;
    }

    public new void Execute(InformationAdapter information)
    {   
        OutputMessage?.Invoke(information.InputText ?? string.Empty);
    }
}
