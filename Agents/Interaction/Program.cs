using Technologai;

namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static TechnologaiAgent? _agent;
        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {   
            var authorityName = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));
            
            _agent = new TechnologaiAgent(authorityName, clientId, clientSecret, memberId);

            _agent.OutputReceived += _agent_OutputReceived;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_OutputReceived(object? sender, string message)
        {
            Console.WriteLine($"{(sender as TechnologaiAgent)?.Name} Output> {message}");
        }

        private static void Input(string message)
        {
            _agent?.Publish(message);
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_agent?.Name} Input> ");
            do
            {
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

                Input(value);

                Console.WriteLine($"{_agent?.Name} Sent> {value}");
            }
            while (true);

        }


    }
}