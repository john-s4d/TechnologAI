using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Core.Debug
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;

        internal string? Authority => _config["authority"]; 
        internal string? ClientSecret => _config["clientSecret"];
        internal string? ClientId => _config["clientId"];
        internal string? MemberId => _config["coordinatorMemberId"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
