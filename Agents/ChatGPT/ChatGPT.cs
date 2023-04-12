using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Technologai;

namespace Technologai.Agents.Abilities.ChatGPT
{
    internal class ChatGPT : TechnologaiAgent
    {
        private static OpenAI _openAI = new OpenAI();

        public ChatGPT(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        { }

        public async override Task Handle(Information information)
        {
            Console.WriteLine($"{Name} Received> {information.Input} | {information.Output}");

            await Publish(Spawn(information, "woot woot"));

            Close(information, await _openAI.GetGpt3Response(information.Input));
            
        }
       
    }
}
