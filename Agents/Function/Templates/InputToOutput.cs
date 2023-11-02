namespace Technologai.Templates
{
    internal class InputToOutput : Template
    {
        public InputToOutput()
        {
            Id = "input_to_output";
            Description = "Make the output the same as the input.";
            OutputKeys = new string[] { "text" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            return Data.Create(information.Input);
        }
    }

}