using Technologai;

namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static Agent? _agent;
        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            _agent = new Agent(
                _config.BrokerHost ?? throw new ArgumentNullException(nameof(_config.BrokerHost)),
                new AgentIdentity
                {
                    Name = "Interaction",
                    ClientId = string.Empty,
                    ApiKey = _config.AgentApiKey,
                    TokenEndpoint = _config.TokenEndpoint
                }                
            );
            
            _agent.OutputReceived += _agent_OutputReceived;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_OutputReceived(object? sender, string message)
        {
            Console.WriteLine($"{(sender as Agent)?.Name} Output> {message}");
        }

        private static void Input(string message)
        {
            _agent?.Input(message);
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