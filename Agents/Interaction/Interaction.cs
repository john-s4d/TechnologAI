using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technologai;

namespace Technologai.Agents.Core.Interaction
{
    internal class Interaction : TechnologaiAgent
    {

        public Interaction(string authorityName, string clientId, string clientSecret, string memberId)
            : base(authorityName, clientId, clientSecret, memberId)
        { }

        public override Task Handle(Information information)
        {   
            Console.WriteLine($"{Name} Handle> {information.State} | {information.Input} | {information.Output}");            
            
            return Task.CompletedTask;
        }
    }
}
