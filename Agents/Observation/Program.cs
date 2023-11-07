using Technologai.Templates;

namespace Technologai.Agents.Monitor
{
    internal class Program
    {
        private static Agent? _agent;

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authority = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var instanceId = _config.InstanceId ?? throw new ArgumentNullException(nameof(_config.InstanceId));
            var instanceSecret = _config.InstanceSecret ?? throw new ArgumentNullException(nameof(_config.InstanceSecret));
            var agentId = _config.AgentId ?? throw new ArgumentNullException(nameof(_config.AgentId));

            _agent = new Agent(authority, instanceId, instanceSecret, agentId);
            _agent.LogMessage += AgentLogMessage_callback;

            DisplayMessage displayMessage = new();
            displayMessage.Message += DisplayMessage_callback;
            _agent.Catalog.Add(displayMessage);

            Console.WriteLine("Loading...");

            await _agent.Start();

            do { await Task.Delay(10); } while (true);

            await _agent.Stop();
        }

        private static void DisplayMessage_callback(object? sender, string message)
        {
            Console.WriteLine($"{message}");
        }

        private static void AgentLogMessage_callback(object? sender, string message)
        {

            Console.WriteLine($"{(_agent?.Name ?? "Observation.Local").PadRight(21)} | {message}");
        }
    }
}