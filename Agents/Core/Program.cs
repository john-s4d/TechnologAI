
using Technologai.Agents.Core.Templates;

namespace Technologai.Agents
{
    internal class Program
    {

        private static Agent? _agent;        

        private static AppConfig _config = new AppConfig();

        internal static async Task Main(string[] args)
        {
            var authUri = _config.AuthUri ?? throw new ArgumentNullException(nameof(_config.AuthUri));
            var clientId = _config.ClientId ?? throw new ArgumentNullException(nameof(_config.ClientId));
            var clientSecret = _config.ClientSecret ?? throw new ArgumentNullException(nameof(_config.ClientSecret));
            var memberId = _config.MemberId ?? throw new ArgumentNullException(nameof(_config.MemberId));

            _agent = new Agent(authUri, clientId, clientSecret, memberId);
            _agent.StatusMessage += _agent_StatusMessage;

            _agent.Catalog.Add(new AppendToFile());
            _agent.Catalog.Add(new Generate32BitString());
            _agent.Catalog.Add(new RespondBar());

            Console.WriteLine("Loading...");

            await _agent.Start();

            do { await Task.Delay(10); } while (true);

            await _agent.Stop();
        }

        private static void _agent_StatusMessage(object? sender, string message)
        {

            Console.WriteLine($"{_agent?.Name ?? "Core.Local"} | {message}");
        }
    }       
}