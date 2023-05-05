import base64
import hashlib
import time
from dataclasses import dataclass

@dataclass
class ContextId:
    _id: str
    _unix_timestamp_bytes: bytes = None
    _hash_compute_bytes: bytes = None

    def __init__(self, context_id=None, unix_timestamp=None, id_hash=None):
        if context_id:
            self._id = context_id
            context_bytes = base64.urlsafe_b64decode(self._id)
            self._unix_timestamp_bytes = context_bytes[:8]
            self._hash_compute_bytes = context_bytes[8:]
        elif unix_timestamp and id_hash:
            self._unix_timestamp_bytes = unix_timestamp.to_bytes(8, "big")
            self._hash_compute_bytes = hashlib.md5(id_hash + self._unix_timestamp_bytes).digest()[:8]
            self._id = base64.urlsafe_b64encode(self._unix_timestamp_bytes + self._hash_compute_bytes).decode()

    @staticmethod
    def create(creator_id_base64):
        return ContextId(unix_timestamp=ContextId.get_timestamp_ticks_bytes(), id_hash=ContextId.get_base64_bytes(creator_id_base64, 8))

    @staticmethod
    def get_timestamp_ticks_bytes():
        return int(time.time() * 1000)

    @staticmethod
    def get_base64_bytes(creator_id_base64, count):
        return base64.urlsafe_b64decode(creator_id_base64)[:count]

    def compare_to(self, other):
        if other is None:
            return 1

        result = int.from_bytes(self._unix_timestamp_bytes, "big") - int.from_bytes(other._unix_timestamp_bytes, "big")

        if result == 0:
            return int.from_bytes(self._hash_compute_bytes, "big") - int.from_bytes(other._hash_compute_bytes, "big")

        return result

    def __str__(self):
        return self._id

# Example usage
context_id = ContextId.create("your_base64_creator_id")
print(context_id)
