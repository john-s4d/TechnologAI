using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static Interaction? _agent;
        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authorityName = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _agent = new Interaction(authorityName, clientId, clientSecret, memberId);

            _agent.StatusMessage += _agent_statusMessage;
            _agent.OutputMessage += _agent_outputMessage;

            Console.WriteLine("Loading...");

            await _agent.Start();

            await _agent.Create("get_user_input", "Input>").Publish();
            do
            {
                Thread.Sleep(1000);
            } while (true);
        }

        private static void Input(string input)
        {
            _ = _agent?.Publish(_agent.Create(input)) ?? throw new ArgumentNullException(nameof(_agent));
        }

        private static void _agent_outputMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name} Output> {message}");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name} Status> {message}");
        }


    }
}