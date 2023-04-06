using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Core.Interaction
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;
        
        internal string? BrokerHost => _config["brokerHost"];        
        internal string? AgentApiKey => _config["agentApiKey"]; // TODO: switch to client credentials
        internal string? ClientId => _config["clientId"];
        internal string? ClientSecret => _config["clientSecret"];
        internal string? TokenEndpoint => _config["tokenEndpoint"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
