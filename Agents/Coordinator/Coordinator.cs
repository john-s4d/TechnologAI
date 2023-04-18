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
        /*
        protected override Task Assess(InformationAdapter information, List<Information>? context)
        {
            return Task.CompletedTask;
        }*/

        protected override Task<bool> Assess(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            throw new NotImplementedException();
        }

        protected override Task<Information> Execute(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            throw new NotImplementedException();
        }

        protected override Task<List<Information>> Spawn(InformationAdapter information, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            throw new NotImplementedException();
        }
        /*
protected override Task Execute(InformationAdapter information)
{
   return Task.CompletedTask;
} */
    }
}
