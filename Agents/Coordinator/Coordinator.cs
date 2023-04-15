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
           
        }

        protected override Task Assess(InformationAdapter information)
        {
            Console.WriteLine($"{Name} Compile> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }

        protected override Task Execute(Ability ability, InformationAdapter information)
        {
            Console.WriteLine($"{Name} Execute> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }

        protected override Task Review(InformationAdapter information)
        {
            Console.WriteLine($"{Name} Review> {information.ContextId} | {information.Input} | {information.Output}");

            return Task.CompletedTask;
        }
    }
}
