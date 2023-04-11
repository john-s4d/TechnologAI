using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    internal class ActionCatalog
    {

        public List<Action> Actions { get; private set; } = new List<Action>();


        public ActionCatalog()
        {
            Actions.AddRange(DefaultActions());
        }

        private List<Action> DefaultActions()
        {
            return new List<Action>
            {
                new Action()
                {
                    Name = "add_action_to_catalog",
                    SchemaIn = "{name:string,schemaIn:schema,schemaOut:schema,description:string,doneWhen:string,memberId:id}",
                    SchemaOut = "{success:bool}",
                    Description = "Add an Action to the local Action Catalog",
                    DoneWhen = "(success=true)"
                },

                new Action()
                {
                    Name = "find_action_in_catalog",
                    SchemaIn = "search:string,hints:string",
                    SchemaOut = "[name:string,schemaIn:schema,schemaOut:schema,description:string,doneWhen:string,memberId:id]",
                    Description = "Find and return Actions in the local Action Catalog based on the search string",
                    DoneWhen = "(name.count>0)",
                }
            };
        }

        public void Add_Action(Action action)
        {
            Actions.Add(action);
        }

    }
}
