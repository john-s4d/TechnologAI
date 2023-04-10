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

            _agent.MessageReceived += _agent_MessageReceived;
            _agent.StatusMessage += _agent_statusMessage;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_MessageReceived(object? sender, Message message)
        {
            Console.WriteLine($"{_agent?.Name} Received> {message.Payload}");
        }

        private static void _agent_statusMessage(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Interaction"} Status> {message}");
        }

        private static void Input(string input)
        {
            if (_agent == null) { return; }

            var message = new Message(_agent.Identity)
            {
                Payload = input,
                Context = Context.Create()
            };
            
            _agent?.PublishMessage(message);
            Console.WriteLine($"{_agent?.Name} Published> {message.Payload}");
        }

        private async static Task Run()
        {
            Console.WriteLine($"{_agent?.Name} Started. Enter Input or \"quit\" to stop.");
            do
            {
                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? "";
                });

                if (value.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
                {

                    Console.WriteLine(Utils.GenerateNewIdString(32));
                    continue;
                }

                Input(value);
            }
            while (true);

        }


    }
}