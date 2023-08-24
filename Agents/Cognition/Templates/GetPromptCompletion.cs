namespace Technologai
{
    internal class GetPromptCompletion : Template
    {
        private const string DEFAULT_MODEL = "openai.gpt4";
        public GetPromptCompletion(Agent agent)
        {
            Id = "get_prompt_completion";
            Description = "Get a prompt completion from an LLM Model.";
            InputKeys = new string[] { "model", "prompt" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public override Task<Data?> Process(Information information)
        {            
            if (information.Input == null)
            {
                return Task.FromResult((Data?)null);
            }

            else if (information.Input.Format == DataFormat.STRUCTURED)
            {
                string model = information.Input.Structured?["model"] ?? DEFAULT_MODEL;
                string prompt = information.Input.Structured?["prompt"] ?? string.Empty;
                //return Task.FromResult(new Data(LLM.GetPromptCompletion(model, prompt)));
                return Task.FromResult((Data?)null);
            }
            else
            {

                //return Task.FromResult(new Data(LLM.GetPromptCompletion(DEFAULT_MODEL, information.Input.Raw)));
                return Task.FromResult((Data?)null);
            }
        }
    }
}