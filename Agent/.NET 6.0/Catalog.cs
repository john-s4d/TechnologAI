using System.Collections.Concurrent;

namespace Technologai
{
    public class Catalog : ConcurrentDictionary<string, Template>
    {
        private Identity _identity;

        public Catalog(Identity identity)
        {
            _identity = identity;
        }

        public void Add(Template template)
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
