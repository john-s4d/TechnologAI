using Technologai;

public class Debug : Template
{
    private Agent _agent;
    public Debug(Agent agent)
    {
        Id = "debug";
        Description = "Debug";
        _agent = agent;
    }

    public override Task<bool> Assess(Information information) => Task.FromResult(true);

    public override Task<Data?> Process(Information information)
    {

#if DEBUG

        // Parse the input for the template id and user data

        int firstSpace = information.Input?.Raw?.IndexOf(' ') ?? -1;

        if (firstSpace > 6)
        {
            var templateId = information.Input?.Raw?.Substring(6, firstSpace - 6);
            var userData = information.Input?.Raw?.Substring(firstSpace + 1);

            if (string.IsNullOrEmpty(templateId) || !_agent.Catalog.ContainsKey(templateId) || string.IsNullOrEmpty(userData))
            {
                return Task.FromResult((Data?)null);
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

            information.Publish(DebugCallback, templateId, data);

            return Task.FromResult((Data?)null);
        }
        else
        {
            // TODO: Allow parameterless debug with no data
            return Task.FromResult((Data?)null);
        }

#endif
        return Task.FromResult(new Data("Debug not enabled."));        
    }

    private void DebugCallback(Information information)
    {
        information.Publish(null, "show_output_to_user", information.Output);        
    }
}
