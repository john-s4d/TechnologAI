namespace Technologai
{
    public class ProcessCatalog : Dictionary<string, Process>
    {
        private Identity _identity;
        private TechnologaiAgent _agent;

        public ProcessCatalog(Identity identity, TechnologaiAgent agent)
        {
            _identity = identity;
            _agent = agent;
        }

        // Processes received from other agents. Should all have a MemberId. No DefaultState.
        internal void Add(IProcess process)
        {            
            if (!string.IsNullOrEmpty(process.Id) && !this.ContainsKey(process.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                Add(process.Id, (Process)process);
            }
        }

        public void Add(Process process)
        {
            if (!string.IsNullOrEmpty(process.Id) && !this.ContainsKey(process.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                Add(process.Id, process);
            }
        }
    }
}
