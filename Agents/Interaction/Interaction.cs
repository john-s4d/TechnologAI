using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {   

        public Interaction(string authUri, string clientId, string clientSecret, string memberId)
            : base(authUri, clientId, clientSecret, memberId)
        {
            /*
            // TODO: This ability should be stored on ChatGPT Member and advised after joining agency.
            Processes.Add(
                new Process("choose_agency_ability",
                            "Choose an agency-wide ability to use for the response.",
                            "{\"name\":\"string\"}",                    
                            "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q")
            );

            Processes.Add(
               new Process("chatgpt_prompt")
               {   
                   Description = "Send a prompt to ChatGPT and receive a response.",
                   MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
               }
           );*/
        }

        public class choose_agency_ability_response
        {
            public string? name { get; set; }
        }


        // In Asses, we are compiling Output from incoming information into output ready for the parent.
        // Execute or Spawn()  
       
        // In Execute, we convert input to output using an ability.
        // Or, invoke another ability or set of abilities.
        // Close() should always be called.     
    }
}
