using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class AbilityCatalog : Dictionary<string, IAbility>
    {
        private Identity _identity;

        public AbilityCatalog(Identity identity)
        {
            _identity = identity;
            
            Add(new Ability("add_ability_to_catalog")
            {   
                SampleJsonIn = "{name:string,schemaIn:schema,schemaOut:schema,description:string,doneWhen:string,memberId:id}",
                SampleJsonOut = "{\"success\":true}",
                Description = "Add an ability to the local action catalog.",                
            });

            Add(new Ability("find_an_ability_in_catalog")
            {                
                SampleJsonIn = "search:string,hints:string",
                SampleJsonOut = "{[\"name\":\"string\",\"schemaIn\":\"schemaOut\":\"schema\",\"description\":string\",\"doneWhen\":\"string\",\"memberId\":\"id\"]}",
                Description = "Find and return Actions in the local Action Catalog based on the search string",                
            });
        }

        public void Add(IAbility ability)
        {   
            if (!string.IsNullOrEmpty(ability.Id))
            {
                Add(ability.Id, ability);
            }
        }

        public void AddRange(IEnumerable<IAbility> abilities)
        {
            foreach(Ability ability in abilities)
            {
                Add(ability);
            }
        }
    }
}
