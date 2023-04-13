using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Technologai;

namespace Technologai.Agents.Core.Coordinator
{
    internal class Coordinator : TechnologaiAgent
    {
        public Coordinator(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        {
            Abilities.Add(
                new Ability()
                {
                    Name = "choose_ability",
                    SchemaIn = "{\"input\":string,\"abilities\":[{\"name\":string,\"description\":string}]}",
                    SchemaOut = "{\"name\":string}",
                    Description = "From the provided list, choose the ability to use for the response.",
                    MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
                }
            );

            Abilities.Add(
                new Ability()
                {
                    Name = "chatgpt_prompt",
                    Description = "Send a prompt to ChatGPT and receive a response.",
                    SchemaIn = "{\"input\":string}",
                    SchemaOut = "{\"name\":string}",                    
                    MemberId = "S6MbUNVhvXhClcJT5o3vdD8RDcx1dEkOWN69uzxEJ-Q"
                } 
            );
        }

        public override Task Execute(Ability ability, InformationHandler information)
        {
            Console.WriteLine($"{Name} Execute> {information.ContextId} | {information.AbilityName} | {information.Input}");

            return Task.CompletedTask;
        }

        public override async Task Handle(InformationHandler information)
        {
            Console.WriteLine($"{Name} Handle> {information.ContextId} | {information.Input} | {information.Output}");

            // If we've received a message, it's looking for an owner. Find out how to handle it.
            await information.Spawn(Abilities["choose_ability"]).Publish();
            
        }
    }
}
