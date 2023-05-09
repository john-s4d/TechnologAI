using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Core.Interaction
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;

        internal string? AuthUri => _config["authUri"];        
        internal string? ClientId => _config["clientId"];
        internal string? ClientSecret => _config["clientSecret"];        
        internal string? MemberId => _config["interactionMemberId"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
