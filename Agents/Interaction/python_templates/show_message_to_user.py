from ....Agent.python_agent.information import Information

class ShowMessageToUser:
    def __init__(self, message_callback):
        self.id = "show_message_to_user"
        self.description = "Show a message to the user."
        self.message_callback = message_callback

    async def assess(self, information: Information):
        return True

    async def process(self, information: Information):
        self.message_callback(information.input.raw if information.input else '')
        return None