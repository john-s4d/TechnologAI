using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Technologai;

namespace Interaction.Abilities
{
    internal class ReadFile : IExecute
    {        
        public string? Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? SampleJsonIn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? SampleJsonOut { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<Dictionary<string,object>> Execute(Dictionary<string,object> input)
        {
            return Task.FromResult(input);
        }
    }
}
