namespace Technologai.Templates
{
    internal class DisplayMessage : Template
    {
        public event EventHandler<string>? Message;

        public DisplayMessage()
        {
            Id = "monitor.display_message";
            Description = "Display a message on the monitor screen.";
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override Task<Data?> Process(Information information)
        {
            var literal =  Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(information?.Input?.Raw ?? string.Empty, false);
            Message?.Invoke(information?.CreatorId, literal);
            return Task.FromResult((Data?)null);
        }
    }
}