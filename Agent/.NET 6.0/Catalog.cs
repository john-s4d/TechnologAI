namespace Technologai
{
    public class Catalog : Dictionary<string, ITemplate>
    {
        private Identity _identity;
        private Agent _agent;

        public Catalog(Identity identity, Agent agent)
        {
            _identity = identity;
            _agent = agent;
        }

        public void Add(ITemplate template)
        {
            if (!string.IsNullOrEmpty(template.Id)) // TODO: clustered embeddings for fuzzy lookup
            {
                if (template.MemberId == null)
                {
                    template.MemberId = _identity.Id;
                }

                this[template.Id] = template;
            }
        }
    }
}
