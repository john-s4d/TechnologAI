using Technologai;

internal class InputToOutput : Template
{
    public InputToOutput()
    {
        Id = "input_to_output";
        Description = "Make the output the same as the input.";
    }

    public override Task<bool> Assess(Information information) => Task.FromResult(true);

    public override Task<Data?> Process(Information information)
    {   
        return Task.FromResult(information.Input);
    }
}
