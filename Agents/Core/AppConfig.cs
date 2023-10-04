using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Core
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;

        internal string? AuthUri => _config["authUri"];        
        internal string? ClientId => _config["clientId"];
        internal string? ClientSecret => _config["clientSecret"];
        internal string? MemberId => _config["memberId"];
        internal string? JiraUsername => _config["jiraUsername"];
        internal string? JiraPassword => _config["jiraPassword"];
        internal string? SearchGoogleApiKey => _config["searchGoogleApiKey"];
        internal string? TrascriptAudioVoiceFileApiKey => _config["trascriptAudioVoiceFileApiKey"];
        internal string? OpenAiApiKey => _config["openAiApiKey"];
        internal string? OpenAiUrl => _config["openAiUrl"];
        internal string? OpenAiOrgId => _config["openAiOrgId"];        
        internal string? ReceiveTranscriptApiKey => _config["receiveTranscriptApiKey"];
        
        //Reddit Credentials
        internal string? RedditUsername => _config["redditUsername"];
        internal string? RedditPassword => _config["redditPassword"];
        internal string? RedditClientId => _config["redditClientId"];
        internal string? RedditClientSecret => _config["redditClientSecret"];
        internal string? SubReddit => _config["subReddit"];
        internal string? RedditAppName => _config["redditAppName"];
        
        //Twitter credentials
        internal string? TwitterConsumerKey => _config["twitterConsumerKey"];
        internal string? TwitterConsumerKeySecret => _config["twitterConsumerKeySecret"];
        internal string? TwitterAccessToken => _config["twitterAccessToken"];
        internal string? TwitterAccessTokenSecret => _config["twitterAccessTokenSecret"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
