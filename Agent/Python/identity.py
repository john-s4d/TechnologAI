import base64
import json
import requests
import jwt
from jwt.algorithms import Algorithm

class Identity:
    def __init__(self, authority_name, client_id, client_secret, member_id):
        self.authority = Authority(authority_name)
        self.client_id = client_id
        self.client_secret = client_secret
        self.id = member_id
        self.agency_id = None
        self.name = None
        self.token = None

    @property
    def publish_mask(self):
        return f"{self.agency_id}/+"

    @property
    def subscribe_member_mask(self):
        return f"{self.agency_id}/{self.id}"

    @property
    def subscribe_agency_mask(self):
        return f"{self.agency_id}/0"

    def authenticate(self):
        headers = {
            "Authorization": f"Bearer {base64.urlsafe_b64encode(f'{self.client_id}:{self.client_secret}'.encode()).decode()}",
            "Accept": "application/json"
        }
        parameters = {
            "grant_type": "client_credentials",
            "scope": f"member:{self.id}"
        }

        response = requests.post(self.authority.token_endpoint, json=parameters, headers=headers)

        if response.status_code == 200:
            token_response = response.json()

            if token_response:
                self.token = token_response["access_token"]
                decoded_token = jwt.decode(self.token, options={"verify_signature": False})

                self.agency_id = decoded_token.get("agency_id")
                self.name = decoded_token.get("name")
        else:
            raise Exception("Unauthorized", response.status_code)

    def get_masked_topic(self, topic):
        topic_parts = topic.split("/")
        mask_parts = self.publish_mask.split("/")

        if len(topic_parts) != 2:
            raise ValueError("Invalid topic")

        for i in range(len(topic_parts)):
            topic_parts[i] = mask_parts[i] if mask_parts[i] != "+" else topic_parts[i]

        return "/".join(topic_parts)

class Authority:
    def __init__(self, authority_name):
        self.token_endpoint = f"https://{authority_name}/oauth2/token"

# Example usage
identity = Identity("your_authority_name", "your_client_id", "your_client_secret", "your_member_id")
identity.authenticate()
print(identity.get_masked_topic("your/topic"))
