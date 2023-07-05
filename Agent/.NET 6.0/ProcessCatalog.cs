namespace Technologai
{
    public class ProcessCatalog : Dictionary<string, Neuron>
    {
        private Identity _identity;
        private TechnologaiAgent _agent;

        public ProcessCatalog(Identity identity, TechnologaiAgent agent)
        {
            _identity = identity;
            _agent = agent;
        }

        // Processes received from other agents. Should all have a MemberId. No DefaultState.
        internal void Add(INeuron process)
        {            
            if (!string.IsNullOrEmpty(process.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                this[process.Id] = (Neuron)process;
            }
        }

        public void Add(Neuron process)
        {
            if (!string.IsNullOrEmpty(process.Id)) // TODO: clustered embeddings for fuzzy lookup
            {   
                this[process.Id] = process;
            }
        }
    }
}
