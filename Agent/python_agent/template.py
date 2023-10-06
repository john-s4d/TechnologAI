from typing import List, Optional
from .information import Information

class Template:
    def __init__(self):
        self.id: Optional[str] = None
        self.description: Optional[dict] = None
        self.input_keys: Optional[List[str]] = None
        self.output_keys: Optional[List[str]] = None
        self.member_id: Optional[str] = None

    async def assess(self, information: Information) -> bool:
        return False

    async def process(self, information: Information) -> Optional[dict]:
        return None