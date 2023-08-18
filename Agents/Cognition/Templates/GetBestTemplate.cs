namespace Technologai
{
    internal class GetBestTemplate : Template
    {
        private string _defaultTemplateId;

        public GetBestTemplate(string defaultTemplateId = "input_to_output")
        {
            _defaultTemplateId = defaultTemplateId;

            Id = "get_best_template";
            Description = "Get the Id for the best template to handle the input.";
            OutputKeys = new string[] { "Id" };
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {            
            string? templateId = null; 

            if (information.Input?.Raw == "32bit")
            {                
                templateId = "generate_32_bit_string";
            }

            if (information.Input?.Raw == "foo")
            {
                templateId = "respond_bar";
            }

            if (templateId == null)
            {
                // TODO: Ask an LLM to determine the best template to use from the available templates

                string prompt = $"{information.Input}";

                templateId = await information.Publish("get_prompt_completion", prompt);
            }                    

            return new Data(new Dictionary<string, string> { { "Id", templateId ?? _defaultTemplateId } });
        }
    }
}

/*
var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
$"{JsonConvert.SerializeObject(choose_ability)}" +
$"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
$"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Template.SampleJsonOut}";
*/