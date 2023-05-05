from dataclasses import dataclass

@dataclass
class Authority:
    host: str = "auth.technologai.com"
    client_endpoint: str = None
    token_endpoint: str = None
    broker_host: str = None

    def __init__(self, host):
        # TODO: Connect to Discovery Endpoint and get the correct values
        if host != "auth.technologai.com":
            raise NotImplementedError()

        self.host = host
        self.broker_host = "broker.technologai.com"
        self.client_endpoint = "https://auth.technologai.com/client"
        self.token_endpoint = "https://auth.technologai.com/token"

# Example usage
authority = Authority("auth.technologai.com")
print(authority.host)
print(authority.client_endpoint)
print(authority.token_endpoint)
print(authority.broker_host)
