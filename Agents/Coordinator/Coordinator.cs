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

        protected Task<Assessment> Assess(InformationAdapter information, Assessment assessment)
        {
            throw new NotImplementedException();
        }

        protected Task<Information> Execute(InformationAdapter information, Assessment assessment)
        {
            throw new NotImplementedException();
        }

        protected Task<List<Information>> Spawn(InformationAdapter information, Assessment assessment)
        {
            throw new NotImplementedException();
        }
    }
}
