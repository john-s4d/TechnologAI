namespace Technologai
{
    public class Authority
    {
        public string Host { get; private set; } = "auth.technologai.com";
        public string? ClientEndpoint { get; private set; }
        public string? TokenEndpoint { get; private set; }
        public string BrokerHost { get; private set; }
        public Authority(string host)
        {
            // TODO: Connect to Discovery Endpoint and get the correct values
            if (host != "auth.technologai.com") { throw new NotImplementedException(); }

            this.Host = host;
            this.BrokerHost = "broker.technologai.com";
            this.ClientEndpoint = "https://auth.technologai.com/client";
            this.TokenEndpoint = "https://auth.technologai.com/token";
        }
    }
}
