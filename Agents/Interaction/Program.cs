using Technologai;

namespace Technologai.Agents.Core.Interaction
{
    internal class Program
    {
        private static AgencyMember? _agent;
        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            if (_config.Authority == null) { throw new ArgumentNullException(nameof(_config.Authority)); }
            if (_config.ClientId == null) { throw new ArgumentNullException(nameof(_config.ClientId)); }
            if (_config.ClientSecret == null) { throw new ArgumentNullException(nameof(_config.ClientSecret)); }
            if (_config.MemberId == null) { throw new ArgumentNullException(nameof(_config.MemberId)); }
            if (_config.AgencyId == null) { throw new ArgumentNullException(nameof(_config.AgencyId)); }


            var authority = new Authority(_config.Authority);
            var agentIdentity = new AgentIdentity(_config.ClientId, _config.ClientSecret, authority);            
            var agencyIdentity = new AgencyIdentity(_config.AgencyId, agentIdentity);
            var memberIdentity = new MemberIdentity(_config.MemberId, agentIdentity, agencyIdentity);

            _agent = new AgencyMember(memberIdentity);
            _agent.OutputReceived += _agent_OutputReceived;

            Console.WriteLine("Loading...");

            await _agent.Start();
            await Program.Run();
            await _agent.Stop();
        }

        private static void _agent_OutputReceived(object? sender, string message)
        {
            Console.WriteLine($"{(sender as AgencyMember)?.Name} Output> {message}");
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