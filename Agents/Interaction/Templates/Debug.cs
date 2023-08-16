using Technologai;

public class Debug : Template
{
    private Agent _agent;
    public Debug(Agent agent)
    {
        Id = "debug";
        Description = "Debug ";
        _agent = agent;
    }

    public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

    public override async Task<Data?> Process(InformationAdapter information)
    {

#if DEBUG

            if (_agent != null && (information.Input?.Raw?.StartsWith("DEBUG:") ?? false))
        {
            int firstSpace = information.Input?.Raw?.IndexOf(' ') ?? -1;

            if (firstSpace > 6)
            {

                var templateId = information.Input?.Raw?.Substring(6, firstSpace - 6);
                var userData = information.Input?.Raw?.Substring(firstSpace + 1);

                if (string.IsNullOrEmpty(templateId) || !_agent.Catalog.ContainsKey(templateId) || string.IsNullOrEmpty(userData))
                {
                    return null;
                }

                ITemplate template = _agent.Catalog[templateId];

                Data data;

                if (template.InputKeys != null && template.InputKeys.Length > 0)
                {
                    data = new Data(userData, DataFormat.STRUCTURED);
                }
                else
                {
                    data = new Data(userData, DataFormat.RAW);
                }

                var debugTemplate = await information.Spawn(templateId, data);
                await debugTemplate.Publish(PublishCallback);

                return null;

            }
        }

#endif

        return new Data("Debug not enabled.");
    }

    private void PublishCallback(InformationAdapter information)
    {
        var showUserOutput = information.Spawn("show_output_to_user", information.Output).Result;
        _ = showUserOutput.Publish();
    }
}
