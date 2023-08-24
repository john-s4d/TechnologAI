namespace Technologai.Agents.Interaction
{
    internal class Program
    {
        private static Agent _agent;
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
                _agent.LogMessage += LogMessage_callback;

                // Add local templates
                _agent.Catalog.Add(new GetInputFromUser());
                _agent.Catalog.Add(new InteractWithUser());
                _agent.Catalog.Add(new Debug(_agent));
                _agent.Catalog.Add(new ShowMessageToUser(ShowMessageToUser_callback));

                await _agent.Start();

                await _agent.PublishAsync(interactWithUser_callback, "interact_with_user", "Ready for Input");
                                
                do { await Task.Delay(10); } while (_isStarted);

                await _agent.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private async static Task interactWithUser_callback(Information information)
        {
            if (information.Output?.Raw?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                _isStarted = false;

                Console.WriteLine($"{_agent?.Name} Shutting Down");
            }
            else
            {
                await _agent.PublishAsync(interactWithUser_callback, "interact_with_user", information.Output) ;
            }
        }

        private static void ShowMessageToUser_callback(string? message)
        {
            Console.Write($"{(string.IsNullOrEmpty(message) ? string.Empty : $"{message}")}");
        }

        private static void LogMessage_callback(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction.Local"} | {message}");
        }
    }
}