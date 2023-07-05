
namespace Technologai.Agents
{
    internal class Program
    {

        private static TechnologaiAgent? _agent;        

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authUri = _config.AuthUri ?? throw new ArgumentNullException(nameof(_config.AuthUri));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _agent = new TechnologaiAgent(authUri, clientId, clientSecret, memberId);
            _agent.StatusMessage += _agent_StatusMessage;

            _agent.Neurons.Add(new AppendToFile());

            Console.WriteLine("Loading...");

            await _agent.Start();

            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_StatusMessage(object? sender, string message)
        {

            Console.WriteLine($"{_agent?.Name ?? "Core.Local"} | {message}");
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_agent?.Name} | Started");

            do
            {
                /*
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });*/

                //if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

            }
            while (true);

        }

    }
        /*
        var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
        $"{JsonConvert.SerializeObject(choose_ability)}" +
        $"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
        $"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Process.SampleJsonOut}";
        */
}