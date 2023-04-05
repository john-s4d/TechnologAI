using Microsoft.Extensions.Configuration;

namespace Technologai.Agents.Core.Interaction
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;
        
        internal string? BrokerHost => _config["brokerHost"];        
        internal string? AgentApiKey => _config["agentApiKey"];
        internal string? TokenEndpoint => _config["tokenEndpoint"];

        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
