using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {
        internal event EventHandler<string>? OutputMessage;
        //internal Queue<InformationAdapter> _informationToBeDispatched;

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
                    Name = "interact_with_user",
                    Description = "Provide the user with information and receive a response from the user."
                }
            );

            Abilities.Add(
                new Ability()
                {
                    Name = "get_user_input",
                    Description = "Receive a response from the user."
                }
            );

            Abilities.Add(
                new Ability()
                {
                    Name = "show_user_output",
                    Description = "Provide the user with information."
                }
            );
        }

        public class choose_agency_ability_response
        {
            public string? name { get; set; }
        }

        // In Execute, we convert input to output using an ability.
        // Or, invoke another ability or set of abilities.
        // Close() should be called.
        /*
        protected async override Task Execute(InformationAdapter information)
        {
            if (information.AbilityName == "get_user_input")
            {
                Console.WriteLine(information.Input);

                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? string.Empty;
                });

                if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(Utils.GenerateNewIdString(32));
                }

                information.Close(value);
            }

            if (information.AbilityName == "show_user_output")
            {

                //information.Close();
            }
        }
        */

        // In Asses, we are compiling Output from incoming information into output ready for the parent.
        // Execute or Spawn()  
        /*
        protected async override Task Assess(InformationAdapter information, List<Information>? context)
        {
       
            if (information.AbilityName == "start_interaction")
            {


            }

            if (information.AbilityName == "interact_with_user")
            {
                if (context == null)
                {
                    await information.Spawn("get_user_input").Publish();
                }
                else
                {   
                    await information.Close(context[0].Output).Publish();
                }                
            }

            if (information.AbilityName == "get_user_input")
            {
                Console.WriteLine(information.Input);

                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? string.Empty;
                });

                
                if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(Utils.GenerateNewIdString(32));
                }

                //await information.Close(value).Publish();
           // }

            
            if (information.AbilityName == "show_user_output")
            {
                OutputMessage?.Invoke(this, information.Input ?? string.Empty);
                await information.Close();
            }
        }*/

        protected override Task<bool> Assess(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            return Task.FromResult(string.IsNullOrEmpty(information.Input));
        }

        protected override async Task<Information> Execute(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            if (information.AbilityName == "get_user_input")
            {
                Console.WriteLine(information.Input);

                string value = await Task.Run(() =>
                {
                    return Console.ReadLine() ?? string.Empty;
                });


                if (value.Equals("32Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(Utils.GenerateNewIdString(32));
                }

                information.Close(value);

            }
            return information;
        }

        protected override Task<List<Information>> Spawn(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            var result = new List<Information>();

            if (information.AbilityName == "interact_with_user")
            {
                result.Add(Create("get_user_input", information.Input));

                //result.Add(await Spawn("get_user_input"));

            }
            return Task.FromResult(result);
        }
    }
}
