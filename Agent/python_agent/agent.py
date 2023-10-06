import asyncio
import datetime
from concurrent.futures import ThreadPoolExecutor
from mqtt_client import MqttClient
from identity import Identity
# from catalog import Catalog
from context import Context
from broker_message import BrokerMessage, AgentMessageType
from information import Information, InformationState
from template import Template
from constants import BROKER_URI

class Agent:
    LOG_MESSAGE_TEMPLATE_ID = "monitor.display_message"

    def __init__(self, auth_uri, client_id, client_secret, member_id, log_message_callback=None, catalog: list = []):
        self.identity = Identity(auth_uri, client_id, client_secret, member_id)
        # self.catalog = Catalog(self.identity)
        # self.catalog = { "identity": self.identity }
        self.catalog = catalog
        self.context = Context(self.identity)
        self.log_message_callback = log_message_callback
        self._output_callbacks = {}
        self._known_agents = {}
        self._mqtt = MqttClient(self.identity, self._mqtt_message_received)
        self._executor = ThreadPoolExecutor(max_workers=10)

    async def _mqtt_message_received(self, broker_message):
        # broker_message = BrokerMessage.from_mqtt_args(args)

        if broker_message.message_type == AgentMessageType.PULSE:
            await self.receive(broker_message.message_data)
        elif broker_message.message_type == AgentMessageType.TEMPLATE:
            await self.receive(broker_message.message_data)
        elif broker_message.message_type == AgentMessageType.INFORMATION:
            await self.receive(broker_message.message_data)

    async def receive(self, data):
        if isinstance(data, Template):
            await self._receive_template(data)
        elif isinstance(data, Information):
            await self._receive_information(data)
        else:
            await self._receive_pulse(data)

    async def _receive_pulse(self, pulse):
        if pulse and pulse.member_id != self.identity.id:
            self._known_agents[pulse.member_id] = datetime.datetime.utcnow()

            for template in self.catalog.values():
                if template.member_id == self.identity.id:
                    await self.send(template, AgentMessageType.TEMPLATE, to_member_id=pulse.member_id)

    async def _receive_template(self, template: Template):
        if template and template.member_id != self.identity.id:
            self.catalog.append(template)

    async def _receive_information(self, information):
        if information:
            information.agent = self
            self.context.add(information)

            if information.information_state == InformationState.CLOSED and information.creator_id == self.identity.id:
                callback = self._output_callbacks.pop(information.id, None)
                if callback:
                    await callback(information.output)

            if information.information_state == InformationState.OPEN and information.worker_id == self.identity.id:
                if await information.assess():
                    await information.process()

    async def send(self, data, message_type, to_member_id="0"):
        broker_message = BrokerMessage(self.identity, data, to_member_id, message_type)
        await self._mqtt.publish_async(broker_message.topic, broker_message.message_data, broker_message.message_type)

    async def publish_async(self, template_id, callback, input_data=None):
        information = Information(self, template_id, input_data)
        if callback:
            self._output_callbacks[information.id] = callback

        if information.information_state == InformationState.DRAFT:
            information.information_state = InformationState.OPEN

        if information.worker_id is None:
            information.worker_id = self.catalog.get(information.template_id, self.identity.id)

        if information.worker_id == self.identity.id:
            self._executor.submit(self.receive, information)
        else:
            await self.send(information, information.worker_id)

    async def start(self):
        await self.identity.authenticate(BROKER_URI)
        await self._mqtt.connect_async(BROKER_URI)
        await self._mqtt.subscribe_async(self.identity.subscribe_member_mask)
        await self._mqtt.subscribe_async(self.identity.subscribe_agency_mask)
        await self.send({}, AgentMessageType.PULSE)

    async def stop(self):
        await self._mqtt.disconnect_async()
