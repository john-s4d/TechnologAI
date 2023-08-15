namespace Technologai.Cognition
{
    internal class GetBestTemplate : Template
    {
        public GetBestTemplate()
        {
            Id = "get_best_template";
            Description = "Get the best template to respond to the input.";
            OutputKeys = new string[] { "Id" };            
        }

        public override Task<bool> Assess(InformationAdapter information) => Task.FromResult(true);

        public override Task<Data?> Process(InformationAdapter information)
        {            
            string templateId = "echo_user_input";

            if (information.Input?.Raw == "32bit")
            {                
                templateId = "generate_32_bit_string";
            }

            if (information.Input?.Raw == "foo")
            {
                templateId = "respond_bar";
            }

            // TODO: SECURITY - Only do this in debug mode
            if (information.Input?.Raw?.StartsWith("Template: ") ?? false)
            {
                string[] parts = information.Input?.Raw?.Split(" ") ?? new string[] { };

                templateId = information.Input?.Raw?.Substring(10) ?? templateId;

                // TODO: How to pass Data when testing specific templates?
            }

            return Task.FromResult((Data?)new Data(new Dictionary<string, Data> { { "Id", templateId } }));
        }
    }
}

/*
var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
$"{JsonConvert.SerializeObject(choose_ability)}" +
$"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
$"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Template.SampleJsonOut}";
*/

