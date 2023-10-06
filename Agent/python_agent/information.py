from typing import Dict, Optional
from .agent import Agent
from enum import Enum

class InformationState(Enum):
    NEW = 0
    ASSESSMENT_QUEUED = 1
    PROCESSING = 2

class TemplateState(Enum):
    NEW = 0
    ASSESSMENT_QUEUED = 1
    PROCESSING = 2

class Information:
    def __init__(self, agent: Agent, template_id: str, input_data: Optional[Dict] = None):
        self.agent = agent
        self.id = None  # This should be generated uniquely
        self.creator_id = None  # This should be set appropriately
        self.worker_id = None
        self.template_id = template_id
        self.information_state = InformationState.NEW  # This should be an enum in a real application
        self.template_state = TemplateState.NEW  # This should be an enum in a real application
        self.input = input_data if input_data else {}
        self.output = {}

    async def assess(self):
        if self.template_state == TemplateState.NEW and self.information_state == InformationState.NEW:
            self.template_state = TemplateState.ASSESSMENT_QUEUED
            await self.agent.catalog.assess(self.template_id, self)

    async def process(self):
        if self.template_state == TemplateState.ASSESSMENT_QUEUED and self.information_state == InformationState.NEW:
            self.template_state = TemplateState.PROCESSING
            self.information_state = InformationState.PROCESSING
            await self.agent.catalog.process(self.template_id, self)
            await self.publish()

    async def publish(self):
        new_info = Information(self.agent, self.template_id, self.output)
        self.agent.context.add(new_info)
        self.agent.context.spawn
        await self.agent.publish(new_info)

    def __lt__(self, other):
        return self.id < other.id
