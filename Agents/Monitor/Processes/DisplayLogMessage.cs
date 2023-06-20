namespace Technologai.Agents
{
    internal class DisplayLogMessage : Process
    {
        internal event Action<string>? LogMessage;

        public DisplayLogMessage()
        {
            Id = "display_log_message";
            Description = "Display a message on the output log screen.";
        }
        public new ProcessState Assess(InformationAdapter information)
        {
            return string.IsNullOrEmpty(information.InputText) ? ProcessState.ASSESS : ProcessState.EXECUTE;
        }

        public new void Execute(InformationAdapter information)
        {
            LogMessage?.Invoke(information?.InputText ?? throw new ArgumentNullException(nameof(information.InputText)));
        }
    }
}
