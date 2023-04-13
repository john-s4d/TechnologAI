using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
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
        {
            Abilities.Add(
                new Ability()
                {
                    Name = "chatgpt_prompt",
                    Description = "Send a prompt to ChatGPT and receive a response.",
                    SchemaIn = "{\"input\":string}",
                    SchemaOut = "{\"output\":string}"
                }
            );
        }

        public override async Task Execute(Ability ability, Information information)
        {
            Console.WriteLine($"{Name} Execute> {information.Input} | {information.Output}");

            if (ability.Name == "chatgpt_prompt" && !string.IsNullOrEmpty(information.Input))
            {
                // TODO: De/Serialize according to schemas
                Close(information, await _openAI.GetGpt3Response(information.Input));                
            }
        }

        public override Task Handle(Information information)
        {
            Console.WriteLine($"{Name} Handle> {information.Input} | {information.Output}");
            return Task.CompletedTask;
        }
    }
}
