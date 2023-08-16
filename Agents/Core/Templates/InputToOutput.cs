using Technologai;

internal class InputToOutput : Template
{
    public InputToOutput()
    {
        Id = "input_to_output";
        Description = "Make the output the same as the input.";
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override Task<Data?> Process(InformationAdapter information)
    {   
        return Task.FromResult(information.Input);
    }
}
