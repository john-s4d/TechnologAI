using Microsoft.IdentityModel.Tokens;

namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {
        internal event EventHandler<string>? OutputMessage;

        public Interaction(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        {
            // TODO: This ability should be stored on ChatGPT Member and advised after joining agency.
            Abilities.Add(
                new Ability()
                {
                    Name = "choose_agency_ability",
                    SampleJsonOut = "{\"name\":\"string\"}",
                    Description = "Choose an agency-wide ability to use for the response.",
                    MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
                }
            );

            Abilities.Add(
               new Ability()
               {
                   Name = "chatgpt_prompt",
                   Description = "Send a prompt to ChatGPT and receive a response.",                   
                   MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
               }
           );
            

            // Local Abilities
            Abilities.Add(
                new Ability()
                {
                    Name = "get_user_input",
                    Description = "Ask the user for information."
                }
            );

            Abilities.Add(
                new Ability()
                {
                    Name = "give_user_output",
                    Description = "Provide the user with information."
                }
            );
        }

        public class choose_agency_ability_response
        {
            public string? name { get; set; }

        }


        // In Execute, we convert input to output using our ability.                        
        // Information.Close(output) should always be called.
        // The primary thing here is to put the input into the correct format.
        // Here you can Execute other abilities to get the information you need as well.

        protected override async Task Execute(Ability ability, InformationAdapter information)
        {

            if (ability.Name == "get_user_input")
            {
                Console.WriteLine(information.Input);
                await Task.Run(() =>
                {
                    information.Close(Console.ReadLine());
                });
            }
            /*
            if (ability.Name == "give_user_output")
            {
                OutputMessage?.Invoke(this, information.Output ?? string.Empty);
                information.Close(information.Output);
            }*/
        }

        // TODO: Only handle abilities that are local.
        // In Asses, we are converting Output into usable Input
        // Here you can Execute other abilities if you need help to examine the input.

        protected async override Task Assess(InformationAdapter information)
        {
            //Console.WriteLine($"{Name} Assess> {information.ContextId} | {information.Input} | {information.Output}");

            if (information.AbilityName == "give_user_output")
            {   
                await information.Spawn("get_user_input", information.Output).Publish();
            }            
            /*
            if (information.AbilityName == "get_user_input")
            {
                await information.Spawn("give_user_output", information.Output).Publish();
            }*/

            /*
            if (information.AbilityName == "get_user_input")
            {
                await information.Spawn("choose_agency_ability", information.Output).Publish();
            }           
            
            if (information.AbilityName == "choose_agency_ability")
            {
                var chosenAbilityName = information.DeserializeOutput<choose_agency_ability_response>()?.name ?? string.Empty;
                await information.Spawn(chosenAbilityName, information.Input).Publish();
            }*/
        }

        // In Review, we examine the input and output and use the information to improve
        // Create a new ability?
        protected override Task Review(InformationAdapter information)
        {
            //Console.WriteLine($"{Name} Review> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }
    }
}
