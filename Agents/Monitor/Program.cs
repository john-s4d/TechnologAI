using Technologai.Templates;

namespace Technologai.Agents.Monitor
{
    internal class Program
    {
        private static Agent? _agent;

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authUri = _config.AuthUri ?? throw new ArgumentNullException(nameof(_config.AuthUri));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _agent = new Agent(authUri, clientId, clientSecret, memberId);
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

            Console.WriteLine($"{(_agent?.Name ?? "Monitor.Local").PadRight(21)} | {message}");
        }
    }
}