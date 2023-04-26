using System.Text.Json;
using System.Text.Json.Serialization;
using QuikGraph;

namespace Technologai
{
    public class InformationWrapper : Information
    {
        public InformationWrapper(string id, string creatorId, string abilityId, 
            InformationState state, string? input = null, string? output = null) 
            : base(id, creatorId, abilityId, state, input, output)
        {
        }
    }
}
