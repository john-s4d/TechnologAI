namespace Technologai.Templates
{
    internal class GetTextLength : Template
    {
        public GetTextLength()
        {
            Id = "get_text_length";
            Description = "Get the number of characters in the raw input.";
            OutputKeys = new string[] { "lenght:int" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            return Data.Create(information.Input?.Raw?.Length.ToString());
        }
    }
}