

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

            _agent.MessageReceived += _agent_MessageReceived;
            _agent.StatusMessage += _agent_StatusMessage;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_StatusMessage(object? sender, string message)        {

            Console.WriteLine($"{_agent?.Name ?? "Coordinator"} Status> {message}");            
        }

        private static void _agent_MessageReceived(object? sender, Message message)
        {
            Console.WriteLine($"{_agent?.Name} Received> {message.Payload}");

            message.MemberId = _config?.InteractionMemberId;

            SendMessage(message);
        }

        private static void SendMessage(Message message)
        {   
            _agent?.PublishMessage(message);
            Console.WriteLine($"{_agent?.Name} Published> {message.Payload}");
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