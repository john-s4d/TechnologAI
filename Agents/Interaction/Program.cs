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
                    ApiKey = _config.AgentApiKey,
                    TokenEndpoint = _config.TokenEndpoint
                }                
            );

            Console.WriteLine($"Loading...");

            await _agent.Start();
            _agent.Output += _agent_Output;

            Run().Wait();

            await _agent.Stop();
        }

        private static void _agent_Output(object? sender, string message)
        {
            Console.WriteLine($"output:> {message}");
        }

        private static void Input(string message)
        {
            _agent?.Input(message);
        }

        private async static Task Run()
        {
            Console.WriteLine("input:>");
            do
            {
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

                Input(value);

                Console.WriteLine($"sent:> {value}");
            }
            while (true);

        }


    }
}