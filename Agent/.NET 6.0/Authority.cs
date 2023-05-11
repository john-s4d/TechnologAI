namespace Technologai
{
    public class Authority
    {
        public string AuthUri { get; private set; } = "https://auth.technologai.com";        
        public string BrokerUri { get; private set; } = "https://broker.technologai.com";
        public string StreamUri { get; private set; } = "https://stream.technologai.com";
        public string? ClientApi { get; private set; }
        public string? TokenApi { get; private set; }
        
        public Authority(string authUri)
        {
            // TODO: Connect to Discovery Endpoint and get the correct values
            if (authUri != "https://auth.technologai.com") { throw new NotImplementedException(); }

            this.AuthUri = authUri;            
            this.ClientApi = "/client";
            this.TokenApi = "/token";
        }
    }
}
