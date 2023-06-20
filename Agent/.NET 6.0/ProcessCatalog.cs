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
        private TechnologaiAgent _agent;

        public ProcessCatalog(Identity identity, TechnologaiAgent agent)
        {
            _identity = identity;
            _agent = agent;

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

        internal async void Add(IProcess process, Boolean broadcast)
        {
            
            if (!string.IsNullOrEmpty(process.Id) && !this.ContainsKey(process.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                Add(process.Id, process);

                if (broadcast && _agent.IsConnected)
                {
                    await _agent.Broadcast(process);
                }
            }
        }

        public void Add(IProcess process)
        {
            Add(process, true);
        }

        public void AddRange(IEnumerable<IProcess> processes, Boolean broadcast)
        {
            foreach (IProcess process in processes)
            {
                Add(process, broadcast);
            }
        }
    }
}
