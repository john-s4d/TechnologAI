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
            var authUri = _config.Authority ?? throw new ArgumentNullException(nameof(_config.Authority));
            var instanceId = _config.InstanceId ?? throw new ArgumentNullException(nameof(_config.InstanceId));
            var instanceSecret = _config.InstanceSecret ?? throw new ArgumentNullException(nameof(_config.InstanceSecret));
            var agentId = _config.AgentId ?? throw new ArgumentNullException(nameof(_config.AgentId));
            
            // Post To Reddit Credentials
            var redditUsername = _config.RedditUsername;
            var redditPassword = _config.RedditPassword;
            var redditClientId = _config.RedditClientId;
            var redditClientSecret = _config.RedditClientSecret;
            var redditSubReddit = _config.SubReddit;
            var redditAppName = _config.RedditAppName;

            // Twitter Credentials
            var twitterConsumerKey = _config.TwitterConsumerKey;
            var twitterConsumerKeySecret = _config.TwitterConsumerKeySecret;
            var twitterAccessToken = _config.TwitterAccessToken;
            var twitterAccessTokenSecret = _config.TwitterAccessTokenSecret;

            var jiraUsername = _config.JiraUsername;
            var jiraPassword = _config.JiraPassword;
            var searchGoogleApiKey = _config.SearchGoogleApiKey;
            var trascriptAudioVoiceFileApiKey = _config.TrascriptAudioVoiceFileApiKey;
            var openAiApiKey = _config.OpenAiApiKey;
            var openAiOrgId = _config.OpenAiOrgId;
            var openAiUrl = _config.OpenAiUrl;
            var receiveTranscriptApiKey = _config.ReceiveTranscriptApiKey;



            _agent = new Agent(authUri, instanceId, instanceSecret, agentId);
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
            _agent.Catalog.Add(new GetCurrentDateTimeUTC());
            //_agent.Catalog.Add(new CsvMerge());
            _agent.Catalog.Add(new DownloadFile());            
            _agent.Catalog.Add(new ExecutePython());
            _agent.Catalog.Add(new ExecuteShell());
            _agent.Catalog.Add(new GenerateAudioVoice());
            _agent.Catalog.Add(new GenerateImageDallE2(openAiUrl, openAiOrgId, openAiApiKey));
            _agent.Catalog.Add(new GenerateImageDallE3(openAiApiKey));
            _agent.Catalog.Add(new GenerateImageStableDiifusion());
            _agent.Catalog.Add(new GetJiraComments(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new UpdateJiraTicket(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new PostJiraComment(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GetJiraTicketById(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GetJiraTickets(jiraUsername, jiraPassword));
            _agent.Catalog.Add(new GitClone());
            _agent.Catalog.Add(new ListFiles());
            _agent.Catalog.Add(new PostToReddit(redditUsername, redditPassword, redditClientId, redditClientSecret, redditSubReddit, redditAppName));
            _agent.Catalog.Add(new PostToTwitter(twitterAccessToken, twitterAccessTokenSecret, twitterConsumerKey, twitterConsumerKeySecret));
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
            Console.WriteLine($"{_agent?.Name ?? "Function.Local"} | {message}");
        }
    }       
}