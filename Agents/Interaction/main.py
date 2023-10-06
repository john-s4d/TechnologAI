import asyncio
from .python_templates import GetInputFromUser, InteractWithUser, Debug, ShowMessageToUser
from ...Agent.python_agent.agent import Agent

import dotenv
import os


class Program:
    agent = None
    is_started = True

    @staticmethod
    async def main():
        dotenv.load_dotenv()
        auth_uri = os.getenv("AUTH_URI")
        client_id = os.getenv("CLIENT_ID")
        client_secret = os.getenv("CLIENT_SECRET")
        member_id = os.getenv("MEMBER_ID")

        try:
            print("Loading...")

            Program.agent = Agent(
                auth_uri,
                client_id,
                client_secret,
                member_id,
                log_message_callback=Program.log_message_callback,
                catalog=[GetInputFromUser(), InteractWithUser(), Debug(Program.agent), ShowMessageToUser(Program.show_message_to_user_callback)]
            )

            # # Add local templates
            # Program.agent.catalog.add(GetInputFromUser())
            # Program.agent.catalog.add(InteractWithUser())
            # Program.agent.catalog.add(Debug(Program.agent))
            # Program.agent.catalog.add(ShowMessageToUser(Program.show_message_to_user_callback))

            await Program.agent.start()

            await Program.agent.publish_async("interact_with_user", Program.interact_with_user_callback, "Ready for Input")

            while Program.is_started:
                await asyncio.sleep(0.01)

            await Program.agent.stop()

        except Exception as ex:
            print(str(ex))

    @staticmethod
    async def interact_with_user_callback(output):
        if output and output.raw.lower() == "quit":
            Program.is_started = False
            print(f"{Program.agent.name if Program.agent else 'Interaction.Local'} Shutting Down")
        else:
            await Program.agent.publish_async("interact_with_user", Program.interact_with_user_callback, output)

    @staticmethod
    def show_message_to_user_callback(message):
        print(message if message else "")

    @staticmethod
    def log_message_callback(sender, message):
        print(f"{Program.agent.name if Program.agent else 'Interaction.Local'} | {message}")


if __name__ == "__main__":
    asyncio.run(Program.main())
