using Microsoft.VisualBasic;

namespace Technologai.Agents
{
    internal class Program
    {
        private static TechnologaiAgent? _agent;
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

                _agent = new TechnologaiAgent(authUri, clientId, clientSecret, memberId);
                _agent.StatusMessage += _agent_statusMessage;

                // Add local neurons
                _agent.Neurons.Add(new GetUserInput());
                _agent.Neurons.Add(new InteractWithUser());
                _agent.Neurons.Add(new ShowUserOutput(showUserOutput_outputMessage));

                await _agent.Start();

                var interact_with_user = await _agent.Create("interact_with_user", "Hello");
                await interact_with_user.Publish(information_OnPublishedCallback);

                do { } while (_isStarted);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static async void information_OnPublishedCallback(InformationAdapter information)
        {
            Console.WriteLine($"{_agent?.Name} Received> {information.OutputText}");

            if (information.OutputText?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                _isStarted = false;
                Console.WriteLine($"{_agent?.Name} Shutting Down");
            }
            else if (_agent != null)
            {
                var interact_with_user = await _agent.Create("interact_with_user", "Hello Again");
                await interact_with_user.Publish(information_OnPublishedCallback);
            }
        }

        private static void showUserOutput_outputMessage(string message)
        {
            Console.WriteLine($"{_agent?.Name}> {message}");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction.Local"} | {message}");
        }
    }
}