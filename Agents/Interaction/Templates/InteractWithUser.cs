namespace Technologai
{
    public class InteractWithUser : Template
    {
        public InteractWithUser()
        {
            Id = "interact_with_user";
            Description = "Provide the user with information and receive a response from the user. Then find and respond with the best template.";
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override async Task<Data?> Process(Information information)
        {
            await information.Publish("show_message_to_user", information.Input);
            await information.Publish("show_message_to_user", "\r\n> ");

            var userInput = await information.Publish("get_input_from_user");

#if DEBUG

            if (userInput?.Raw?.StartsWith("DEBUG:") ?? false)
            {
                return await information.Publish("debug", userInput);
            }
#endif

            var bestTemplate = await information.Publish("get_best_template", userInput);

            return await information.Publish(bestTemplate?.Structured?["Id"] ?? "input_to_output", userInput);

        }
    }
}