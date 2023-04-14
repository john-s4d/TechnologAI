using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class AbilityCatalog : Dictionary<string, Ability>
    {
        private Identity _identity;

        public AbilityCatalog(Identity identity)
        {
            _identity = identity;
            
            Add(new Ability()
            {
                Name = "add_ability_to_catalog",
                SampleJsonIn = "{name:string,schemaIn:schema,schemaOut:schema,description:string,doneWhen:string,memberId:id}",
                SampleJsonOut = "{success:bool}",
                Description = "Add an ability to the local action catalog.",                
            });

            Add(new Ability()
            {
                Name = "find_abilities_in_catalog",
                SampleJsonIn = "search:string,hints:string",
                SampleJsonOut = "[name:string,schemaIn:schema,schemaOut:schema,description:string,doneWhen:string,memberId:id]",
                Description = "Find and return Actions in the local Action Catalog based on the search string",                
            });
        }

        public void Add(Ability ability)
        {   
            if (!string.IsNullOrEmpty(ability.Name))
            {
                Add(ability.Name, ability);
            }
        }

        public void AddRange(IEnumerable<Ability> abilities)
        {
            foreach(Ability ability in abilities)
            {
                Add(ability);
            }
        }
    }
}
