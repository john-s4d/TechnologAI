namespace Technologai
{
    public class NeuronCatalog : Dictionary<string, Neuron>
    {
        private Identity _identity;
        private TechnologaiAgent _agent;

        public NeuronCatalog(Identity identity, TechnologaiAgent agent)
        {
            _identity = identity;
            _agent = agent;
        }

        // Neurons advertised from other agents. Should all have a MemberId.
        internal void Add(INeuron neuron)
        {
            Add((Neuron)neuron);
        }

        public void Add(Neuron neuron)
        {
            if (!string.IsNullOrEmpty(neuron.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                if (neuron.MemberId == null)
                {
                    neuron.MemberId = _identity.Id;
                }

                this[neuron.Id] = neuron;
            }
        }
    }
}
