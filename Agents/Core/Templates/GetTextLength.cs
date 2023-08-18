namespace Technologai
{
    internal class GetTextLength : Template
    {
        public GetTextLength()
        {
            Id = "get_text_length";
            Description = "Get the number of characters in the raw input.";
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override Task<Data?> Process(Information information)
        {
            return Task.FromResult((Data?)new Data(information.Input?.Raw?.Length.ToString()));
        }
    }
}