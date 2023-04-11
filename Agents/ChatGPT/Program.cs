

namespace Technologai.Agents.Abilities.ChatGPT
{
    internal class Program
    {

        private static TechnologaiAgent? _agent;
        private static OpenAI _openAI = new OpenAI();

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authorityName = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _agent = new TechnologaiAgent(authorityName, clientId, clientSecret, memberId);

            _agent.InformationReceived += _agent_InformationReceived;
            _agent.StatusMessage += _agent_StatusMessage;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_StatusMessage(object? sender, string message)
        {

            Console.WriteLine($"{_agent?.Name ?? "ChatGPT"} Status> {message}");
        }

        private static void _agent_InformationReceived(object? sender, Information information)
        {
            Console.WriteLine($"{_agent?.Name} Received> {information.Input} | {information.Output}");
            HandleInformation(information).Wait();
        }

        private static async Task HandleInformation(Information information)
        {   
            if (_agent == null || string.IsNullOrEmpty(information.Input)) { return;  }

            Information information_new = _agent.Spawn(information);
            information_new.Input = "bar";
//            Publish(information_new);
                        
            information.Complete(await _openAI.GetGpt3Response(information.Input));
            Publish(information);
        }

        private static void Publish(Information information)
        {
            _agent?.Publish(information);
            Console.WriteLine($"{_agent?.Name} Published> {information.Input} | {information.Output}");
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_agent?.Name} Started. \"quit\" to stop.");

            do
            {
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase)) { break; }

            }
            while (true);

        }

    }



}