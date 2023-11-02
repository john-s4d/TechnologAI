using Technologai.Templates;
using Technologai.Templates.Jira;

namespace Technologai.Agents.Core
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
            //var jiraUsername = _config.JiraUsername ?? throw new ArgumentNullException(nameof(_config.JiraUsername));
            //var jiraPassword = _config.JiraPassword ?? throw new ArgumentNullException(nameof(_config.JiraPassword));            

            _agent = new Agent(authUri, clientId, clientSecret, memberId);
            _agent.LogMessage += LogMessage_callback;

            _agent.Catalog.Add(new AppendToFile());
            _agent.Catalog.Add(new Generate32BitString());
            _agent.Catalog.Add(new RespondBar());
            _agent.Catalog.Add(new ChunkText());
            _agent.Catalog.Add(new InputToOutput());
            _agent.Catalog.Add(new KillSwitch(_agent));
            _agent.Catalog.Add(new GetTextLength());      
            _agent.Catalog.Add(new DeleteFile());
            //_agent.Catalog.Add(new GetJiraTickets(jiraUsername, jiraPassword));

            Console.WriteLine("Loading...");

            await _agent.Start();

            do { await Task.Delay(10); } while (true);

            await _agent.Stop();
        }

        private static void LogMessage_callback(object? sender, string message)
        {
            Console.WriteLine($"{_agent?.Name ?? "Core.Local"} | {message}");
        }
    }       
}