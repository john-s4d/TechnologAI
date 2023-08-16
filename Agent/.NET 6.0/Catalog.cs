namespace Technologai
{
    public class Catalog : Dictionary<string, ITemplate>
    {
        private Identity _identity;

        public Catalog(Identity identity)
        {
            _identity = identity;
        }

        public void Add(ITemplate template)
        {
            if (!string.IsNullOrEmpty(template.Id))
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
