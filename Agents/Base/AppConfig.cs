using Microsoft.Extensions.Configuration;

namespace Technologai
{
    internal class AppConfig
    {
        private readonly IConfiguration _config;
        
        internal string MqttUsername => _config["mqtt_username"];
        internal string MqttPassword=> _config["mqtt_password"];


        internal AppConfig()
        {
            _config = new ConfigurationBuilder()
            .AddUserSecrets<AppConfig>()
            .Build();
        }
    }
}
