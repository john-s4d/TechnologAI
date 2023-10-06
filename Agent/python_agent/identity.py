import base64
import requests
import jwt

from constants import TOKEN_API

class Identity:
    def __init__(self, auth_uri, client_id, client_secret, member_id):
        self.auth_uri = auth_uri
        self.client_id = client_id
        self.client_secret = client_secret
        self.member_id = member_id
        self.agency_id = None
        self.tokens = {}

    @property
    def name(self):
        if self.id_token:
            return self.id_token.get('name')
        return None

    @property
    def id(self):
        if self.id_token:
            return self.id_token.get('sub')
        return None

    @property
    def subscribe_member_mask(self):
        return f"{self.agency_id}/{self.member_id}"

    @property
    def subscribe_agency_mask(self):
        return f"{self.agency_id}/0"

    async def authenticate(self, audience):
        headers = {
            'Authorization': 'Bearer ' + base64.b64encode(f"{self.client_id}:{self.client_secret}".encode()).decode(),
            'Accept': 'application/json'
        }

        parameters = {
            'grant_type': 'client_credentials',
            'scope': f'member:{self.id}',
            'audience': audience
        }

        endpoint = self.auth_uri + TOKEN_API
        response = requests.post(endpoint, headers=headers, json=parameters)

        if response.status_code == 200:
            token_response = response.json()

            if token_response is not None:
                decoded_token = jwt.decode(token_response['access_token'], options={"verify_signature": False})  # TODO verify signature

                for claim in decoded_token:
                    if claim == 'agency_id':
                        self.agency_id = decoded_token[claim]
                    if claim == 'name':
                        self.name = decoded_token[claim]
                    if claim == 'aud':
                        self.tokens[decoded_token[claim]] = token_response['access_token']
                return

        raise Exception('Unauthorized')
