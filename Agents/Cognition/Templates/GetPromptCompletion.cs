namespace Technologai
{
    internal class GetPromptCompletion : Template
    {
        private const string DEFAULT_MODEL = "openai.gpt4";
        public GetPromptCompletion(Agent agent)
        {
            Id = "get_prompt_completion";
            Description = "Get a prompt completion from an LLM Model";
            InputKeys = new string[] { "model", "prompt" };
        }

        public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

        public override Task<Data?> Process(InformationAdapter information)
        {            
            if (information.Input == null)
            {
                return Task.FromResult((Data?)null);
            }
            else if (information.Input.Format == DataFormat.STRUCTURED)
            {
                var model = information.Input.Structured?["model"] ?? DEFAULT_MODEL;
                var prompt = information.Input.Structured?["prompt"] ?? string.Empty;                
                return Task.FromResult(new Data(LLM.GetPromptCompletion(model, prompt)));
            }
            else
            {
                return Task.FromResult(new Data(LLM.GetPromptCompletion(DEFAULT_MODEL, information.Input.Raw)));
            }
        }
    }
}