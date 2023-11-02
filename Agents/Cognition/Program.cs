using Technologai.Templates;

namespace Technologai.Agents.Cognition
{
    internal class Program
    {

        private static Agent? _agent;

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authUri = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var instanceId = _config.InstanceId ?? throw new ArgumentNullException(nameof(_config.InstanceId));
            var instanceSecret = _config.InstanceSecret ?? throw new ArgumentNullException(nameof(_config.InstanceSecret));
            var agentId = _config.AgentId ?? throw new ArgumentNullException(nameof(_config.AgentId));

            _agent = new Agent(authUri, instanceId, instanceSecret, agentId);
            _agent.LogMessage += LogMessage_callback;

            //_agent.Catalog.Add(new AddTemplateToCatalog());
            //_agent.Catalog.Add(new FindTemplateInCatalog());
            //_agent.Catalog.Add(new GetEmbeddings());
            _agent.Catalog.Add(new GetBestTemplate());
            _agent.Catalog.Add(new GetPromptCompletion(_agent));

            Console.WriteLine("Loading...");

            await _agent.Start();

            do { await Task.Delay(10); } while (true);

            await _agent.Stop();
        }

        private static void LogMessage_callback(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Cognition.Local"} | {message}");
        }
    }
}