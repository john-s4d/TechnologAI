
using Microsoft.VisualBasic;

namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static Interaction? _agent;
        private static AppConfig _config = new AppConfig();
        private static bool _isStarted = true;

        internal static async Task Main(string[] args)
        {
            var authUri = _config.AuthUri ?? throw new ArgumentNullException(nameof(_config.AuthUri));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            try
            {
                Console.WriteLine("Loading...");

                _agent = new Interaction(authUri, clientId, clientSecret, memberId);

                _agent.StatusMessage += _agent_statusMessage;

                _agent.Processes.Add(new GetUserInput());
                _agent.Processes.Add(new InteractWithUser());

                var showUserOutput = new ShowUserOutput();
                showUserOutput.OutputMessage += showUserOutput_OutputMessage;
                _agent.Processes.Add(showUserOutput);

                await _agent.Start();

                _agent.PublishWithCallback(_agent.Create("interact_with_user", "Hello"), information_OnPublishedCallback);

                do { } while (_isStarted);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void information_OnPublishedCallback(InformationAdapter information)
        {
            Console.WriteLine($"{_agent?.Name} Received> {information.Output}");

            if (information.Output?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                _isStarted = false;
            }
            else
            {
                _agent?.PublishWithCallback(_agent.Create("interact_with_user", "Hello Again"), information_OnPublishedCallback);
            }
        }

        private static void showUserOutput_OutputMessage(string message)
        {
            Console.WriteLine($"{_agent?.Name}> {message}");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction.Local"} Status> {message}");
        }
    }
}