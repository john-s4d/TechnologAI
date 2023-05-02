import json
from dataclasses import dataclass
from enum import Enum

class InformationState(Enum):
    DRAFT = 1
    OPEN = 2
    CLOSED = 3

@dataclass
class Information:
    id: str
    creator_id: str
    state: InformationState
    input: str = None
    output: str = None
    process_id: str = None

    @staticmethod
    def create(creator_id, ability_id, input=None):
        return Information(
            ContextId.create(creator_id),
            creator_id,
            ability_id,
            InformationState.DRAFT,
            input,
            None
        )

    @staticmethod
    def from_json(json_str):
        data = json.loads(json_str)
        data["state"] = InformationState[data["state"]]
        return Information(**data)

    def to_json(self):
        data = self.__dict__.copy()
        data["state"] = self.state.name
        return json.dumps(data)

    def compare_to(self, other):
        if other is None:
            return 1
        return ContextId(self.id).compare_to(ContextId(other.id))

# Example usage of the Information class
information = Information.create("creator_id", "ability_id", "input_data")
json_str = information.to_json()
print(json_str)

info_from_json = Information.from_json(json_str)
print(info_from_json)
