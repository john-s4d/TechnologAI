using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Linq;

namespace Technologai.Agents
{
    internal class ChatGPT : TechnologaiAgent
    {
        private static OpenAI _openAI = new OpenAI();

        public ChatGPT(string authUri, string clientId, string clientSecret, string memberId)
            : base(authUri, clientId, clientSecret, memberId)
        {
           
        }


        protected async Task<Dictionary<string, object>> Execute(Dictionary<string,object> data)
        {
            /*
            if (information.ProcessId == "chatgpt_prompt")
            {
                 return await _openAI.GetGpt3Response(information.Input ?? string.Empty);
            }

            if (information.ProcessId == "choose_agency_process")
            {
                choose_agency_ability_input choose_ability = new choose_agency_ability_input();
                choose_ability.input = information.Input;

                foreach (string processId in Processes.Keys)
                {
                    choose_ability.abilities?.Add(new KeyValuePair<string, string>(processId, Processes[processId].Description ?? string.Empty));
                }

                var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
                             $"{JsonConvert.SerializeObject(choose_ability)}" +
                             $"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
                             $"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.Process.SampleJsonOut}";

                return await _openAI.GetGpt3Response(prompt);
            }*/
            return new Dictionary<string, object>();
        }

        public class choose_agency_process_input
        {
          
        }

        protected Task<List<Information>> Spawn(InformationAdapter information)
        {
            throw new NotImplementedException();
        }
    }
}
