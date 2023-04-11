

namespace Technologai.Agents.Core.Coordinator
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

            _agent.InformationReceived += _agent_InformationReceived;
            _agent.StatusMessage += _agent_StatusMessage;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_StatusMessage(object? sender, string message)        {

            Console.WriteLine($"{_agent?.Name ?? "Coordinator"} Status> {message}");            
        }

        private static void _agent_InformationReceived(object? sender, Information information)
        {
            Console.WriteLine($"{_agent?.Name} Received>{information.ContextId}:{information.Input}:{information.Output}");
            HandleInformation(information);            
        }

        private static void HandleInformation(Information information)
        {            
            // TODO: Coordinator things, basically routing
            // Coordinator has the master Member, Actions, and Prompts lists                       

            string CHAT_GPT_ID = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q";
            Publish(information, CHAT_GPT_ID);
        }

        private static void Publish(Information information, string? memberId = null)
        {   
            _agent?.Publish(information, memberId);
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