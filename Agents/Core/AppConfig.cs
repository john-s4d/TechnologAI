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
        internal string? RedditUsername => _config["redditUsername"];
        internal string? RedditPassword => _config["redditPassword"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
