using Technologai;

internal class AppendToFile : Neuron
{
    public AppendToFile()
    {
        Id = "append_to_file";
        Description = "Append text to file in the local filesystem.";
        InputKeys = new string[] { "filename", "content" };
    }

    public override Task<bool> Assess(InformationAdapter information)
    {
        return Task.FromResult(
                information.InputData.ContainsKey("filename") &&
                information.InputData.ContainsKey("content")
        );          
    }

    public override async Task<Data?> Spike(InformationAdapter information)
    {
        using (var writer = new StreamWriter(information.InputData?["filename"]
                   ?? throw new ArgumentNullException("filename"), true))
        {
            await writer.WriteAsync(information.InputData?["content"]);
        }
        return null;
    }
}
