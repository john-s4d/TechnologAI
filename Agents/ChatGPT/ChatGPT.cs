using Newtonsoft.Json;
using System.Linq;

namespace Technologai.Agents.Abilities.ChatGPT
{
    internal class ChatGPT : TechnologaiAgent
    {
        private static OpenAI _openAI = new OpenAI();

        public ChatGPT(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        {
            Abilities.Add(
                new Ability()
                {
                    Name = "chatgpt_prompt",
                    Description = "Send a prompt to ChatGPT and receive a response."                    
                }
            );

            Abilities.Add(
                new Ability()
                {
                    Name = "choose_agency_ability",
                    SampleJsonOut = "{\"name\":\"string\"}",
                    Description = "Choose an agency-wide ability to use for the response."
                }
            );
        }

        public class choose_agency_ability_input
        {
            public string? input { get; set; }
            public List<KeyValuePair<string, string>>? abilities { get; set; } = new List<KeyValuePair<string, string>>();
        }

        public override async Task Execute(Ability ability, InformationHandler information)
        {
            Console.WriteLine($"{Name} Execute> {information.ContextId} | {information.AbilityName} | {information.Input}");

            //-----------------------------------//
            
            if (ability.Name == "chatgpt_prompt")
            {
                information.Close(await _openAI.GetGpt3Response(information.Input ?? string.Empty));
            }
            //-----------------------------------//
            if (ability.Name == "choose_agency_ability")
            {
                choose_agency_ability_input choose_ability = new choose_agency_ability_input();
                choose_ability.input = information.Input;
                foreach(string abilityName in Abilities.Keys)
                {
                    choose_ability.abilities?.Add(new KeyValuePair<string, string>(abilityName, Abilities[abilityName].Description ?? string.Empty));
                }
                
                var prompt = $"Your response MUST be a compliant machine-readable JSON document.\r\n\r\n" +
                             $"{JsonConvert.SerializeObject(choose_ability)}" +
                             $"\r\n\r\nGiven the list of abilities provided, specify which one you would like to use to respond to the input. " +
                             $"Your response should consist of a single JSON object with the name of the selected ability. For example: {information.SampleJsonOut}";

                information.Close(await _openAI.GetGpt3Response(prompt));
            }
            //-----------------------------------//
        }

        public override Task Compile(InformationHandler information)
        {
            Console.WriteLine($"{Name} Compile> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }

        public override Task Review(InformationHandler information)
        {
            Console.WriteLine($"{Name} Review> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }

    }
}
