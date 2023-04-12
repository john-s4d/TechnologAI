using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technologai;

namespace Technologai.Agents.Core.Coordinator
{
    internal class Coordinator : TechnologaiAgent
    {

        public Coordinator(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        { }

        public override async Task Handle(Information information)
        {
            Console.WriteLine($"{Name} Handle> {information.State} | {information.Input} | {information.Output}");

            // TODO: Coordinator things, basically routing
            // Coordinator has the master Member, Actions, and Prompts lists
            // 
            if (information.OwnerId == null)
            {
                string CHAT_GPT_ID = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q";
                Assign(information, CHAT_GPT_ID);
            }
        }
    }
}
