using Microsoft.VisualBasic;

namespace Technologai.Agents
{
    internal class Program
    {
        private static Agent? _agent;
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

                _agent = new Agent(authUri, clientId, clientSecret, memberId);
                _agent.StatusMessage += _agent_statusMessage;

                // Add local templates
                _agent.Catalog.Add(new GetUserInput());
                _agent.Catalog.Add(new InteractWithUser());
                _agent.Catalog.Add(new ShowUserOutput(showUserOutput_outputMessage));

                await _agent.Start();                

                var interact_with_user = await _agent.Create("interact_with_user", "Input");
                await interact_with_user.Publish(information_OnPublishedCallback);
                                
                do { await Task.Delay(10); } while (_isStarted);

                await _agent.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static async void information_OnPublishedCallback(InformationAdapter information)
        {
            Console.WriteLine($"{information.OutputText}");

            if (information.OutputText?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                _isStarted = false;
                Console.WriteLine($"{_agent?.Name} Shutting Down");
            }
            else if (_agent != null)
            {
                var interact_with_user = await _agent.Create("interact_with_user", "Input");
                await interact_with_user.Publish(information_OnPublishedCallback);
            }
        }

        private static void showUserOutput_outputMessage(string message)
        {
            Console.Write($"{message}> ");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction.Local"} | {message}");
        }
    }
}