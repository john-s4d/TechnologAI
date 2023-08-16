namespace Technologai.Cognition
{
    internal class GetBestTemplate : Template
    {
        private string _defaultTemplateId;

        public GetBestTemplate(string defaultTemplateId = "input_to_output")
        {
            Id = "get_best_template";
            Description = "Get the best template for handling the input.";
            OutputKeys = new string[] { "Id" };

            _defaultTemplateId = defaultTemplateId;
        }

        public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

        public override Task<Data?> Process(InformationAdapter information)
        {            
            string templateId = _defaultTemplateId; 

            if (information.Input?.Raw == "32bit")
            {                
                templateId = "generate_32_bit_string";
            }

            if (information.Input?.Raw == "foo")
            {
                templateId = "respond_bar";
            }

            // TODO: Ask an LLM to determine the best template to use

            return Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Id", templateId } }));
        }
    }
}

/*
var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
$"{JsonConvert.SerializeObject(choose_ability)}" +
$"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
$"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Template.SampleJsonOut}";
*/

