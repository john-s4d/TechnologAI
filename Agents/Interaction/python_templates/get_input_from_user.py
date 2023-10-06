from ....Agent.python_agent.information import Information

class GetInputFromUser:
    def __init__(self):
        self.id = "get_input_from_user"
        self.description = "Receive a text input from the user."

    async def assess(self, information: Information):
        return True

    async def process(self, information: Information):
        return input()
