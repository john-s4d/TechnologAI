using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class ProcessCatalog : Dictionary<string, IProcess>
    {
        private Identity _identity;

        public ProcessCatalog(Identity identity)
        {
            _identity = identity;
            /*
            Add(new Process("add_process_to_agency_catalog",
                            "Add a process to the agency's process catalog.",
                            "{\"id\":\"string\",\"sampleJsonIn\":\"string\",\"sampleJsonOut\":\"string\",\"description\":\"string\",\"memberId\":\"string\"}",
                            "{\"success\":\"boolean\"}"
                    ));

            Add(new Process("find_a_process_in_catalog",
                            "Find and return processes from the agency's process catalog, based on a search string.",
                            "{\"search\":\"string\"}",
                            "{\"id\":\"string\",\"sampleJsonIn\":\"string\",\"sampleJsonOut\":\"string\",\"description\":\"string\",\"memberId\":\"string\"}"
                    ));*/
        }

        public void Add(IProcess process)
        {   
            if (!string.IsNullOrEmpty(process.Id))
            {
                Add(process.Id, process);
            }
        }

        public void AddRange(IEnumerable<IProcess> processes)
        {
            foreach(Process process in processes)
            {
                Add(process);
            }
        }
    }
}
