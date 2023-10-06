from ....Agent.python_agent.information import Information

class Debug:
    def __init__(self, agent):
        self.id = "debug"
        self.description = "Debug"
        self._agent = agent

    async def assess(self, information: Information):
        return True

    # TODO in the C# code, there is a IF debug, but I am not sure if Python has an equivalent
    async def process(self, information: Information):
        # Parse the input for the template id and user data
        first_space = information.input.raw.find(' ')

        if first_space > 6:
            template_id = information.input.raw[6:first_space]
            user_data = information.input.raw[first_space + 1:]

            if not template_id or template_id not in self._agent.catalog or not user_data:
                return None

            template = self._agent.catalog[template_id]

            if template.input_keys and len(template.input_keys) > 0:
                data = Data(user_data, DataFormat.STRUCTURED)
            else:
                data = Data(user_data, DataFormat.RAW)

            return await information.publish(template_id, data)
        else:
            # TODO: Allow parameterless debug with no data
            return Data("Not Supported")
