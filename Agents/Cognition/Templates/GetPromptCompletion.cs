namespace Technologai.Cognition
{
    internal class GetPromptCompletion : Template
    {

        //OpenAi
        public GetPromptCompletion()
        {
            Id = "get_prompt_completion";
            Description = "Get a prompt completion from an LLM Model";
            InputKeys = new string[] { "model","prompt" };            
        }

        public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);
        /*
        public override Task<Data?> Process(InformationAdapter information)
        {            
            //OpenAI
        }*/
    }
}

/*
var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
$"{JsonConvert.SerializeObject(choose_ability)}" +
$"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
$"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Template.SampleJsonOut}";
*/

