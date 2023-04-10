using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Abilities.ChatGPT
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;

        internal string? Authority => _config["authority"];        
        internal string? ClientId => _config["clientId"];
        internal string? ClientSecret => _config["clientSecret"];        
        internal string? MemberId => _config["chatGptMemberId"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
