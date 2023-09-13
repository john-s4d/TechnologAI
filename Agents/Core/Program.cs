using Core.Templates.Jira;
using Technologai.Templates;
using Technologai.Templates.Core;

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
            var jiraUsername = _config.JiraUsername ?? throw new ArgumentNullException(nameof(_config.JiraUsername));
            var jiraPassword = _config.JiraPassword ?? throw new ArgumentNullException(nameof(_config.JiraPassword));
            var redditUsername = _config.RedditUsername ?? throw new ArgumentNullException(nameof(_config.RedditUsername));
            var redditPassword = _config.RedditPassword ?? throw new ArgumentNullException(nameof(_config.RedditPassword));
            var searchGoogleApiKey = _config.SearchGoogleApiKey ?? throw new ArgumentNullException(nameof(_config.SearchGoogleApiKey));            
            var trascriptAudioVoiceFileApiKey = _config.TrascriptAudioVoiceFileApiKey ?? throw new ArgumentNullException(nameof(_config.TrascriptAudioVoiceFileApiKey));            
            var generateImageDallE2ApiKey = _config.GenerateImageDallE2ApiKey ?? throw new ArgumentNullException(nameof(_config.GenerateImageDallE2ApiKey));            
            var generateImageDallE3ApiKey = _config.GenerateImageDallE3ApiKey ?? throw new ArgumentNullException(nameof(_config.GenerateImageDallE3ApiKey));            
            var receiveTranscriptApiKey = _config.ReceiveTranscriptApiKey ?? throw new ArgumentNullException(nameof(_config.ReceiveTranscriptApiKey));            

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

            //new Addition
            _agent.Catalog.Add(new GetCurrentDateTime());
            _agent.Catalog.Add(new CsvMerge());
            _agent.Catalog.Add(new DownloadFile());
            _agent.Catalog.Add(new DownloadFile2());
            _agent.Catalog.Add(new ExecutePython());
            _agent.Catalog.Add(new ExecuteShell());
            _agent.Catalog.Add(new GenerateAudioVoice());
            _agent.Catalog.Add(new GenerateImageDallE2(generateImageDallE2ApiKey));
            _agent.Catalog.Add(new GenerateImageDallE3(generateImageDallE3ApiKey));
            _agent.Catalog.Add(new GenerateImageStableDiifusion());
            _agent.Catalog.Add(new GetJiraComments(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new UpdateJiraTicket(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new PostJiraComment(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GetJiraTicketById(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GetJiraTickets(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GitClone());
            _agent.Catalog.Add(new ListFiles());
            _agent.Catalog.Add(new PostToReddit(redditUsername, redditPassword));
            _agent.Catalog.Add(new PostToTwitter());
            _agent.Catalog.Add(new ReadFile());
            _agent.Catalog.Add(new ReadLocalFile());
            _agent.Catalog.Add(new ReadWebPage());
            _agent.Catalog.Add(new ReceiveTranscript(receiveTranscriptApiKey));
            _agent.Catalog.Add(new SearchGoogle(searchGoogleApiKey));
            _agent.Catalog.Add(new TrascriptAudioVoiceFile(trascriptAudioVoiceFileApiKey));
            _agent.Catalog.Add(new WriteFile());

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