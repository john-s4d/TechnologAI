import json
from dataclasses import dataclass

@dataclass
class BrokerMessage:
    information: Information = None
    agency_id: str = None
    member_id: str = None

    @property
    def topic(self):
        return f"{self.agency_id or '-'}/{self.member_id or '-'}"

    @property
    def is_broadcast(self):
        return self.member_id == "0"

    @staticmethod
    def from_mqtt_args(topic, payload):
        topic_parts = topic.split('/')
        information = Information.from_json(payload.decode())

        return BrokerMessage(
            agency_id=topic_parts[0],
            member_id=topic_parts[1],
            information=information
        )

    def __init__(self, identity=None, **kwargs):
        if identity:
            self.agency_id = identity.agency_id
        self.__dict__.update(kwargs)

# Example usage
mqtt_topic = "agency_id/member_id"
mqtt_payload = b'{"id": "1", "creator_id": "2", "process_id": "3", "state": "DRAFT"}'

broker_message = BrokerMessage.from_mqtt_args(mqtt_topic, mqtt_payload)
print(broker_message.topic)
print(broker_message.is_broadcast)
