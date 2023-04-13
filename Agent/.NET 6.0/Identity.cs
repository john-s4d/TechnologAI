using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public enum TechnologaiRole
    {
        agency,
        member,
        agent
    }

    public abstract class Identity
    {
        internal TechnologaiRole AssignedRole { get; set; }
        internal string? Name { get; set; }
        public string Id { get; set; }
        internal abstract TechnologaiRole RoleName { get; }
        internal abstract string PublishMask { get; }
        internal abstract string SubscribeMask { get; }

        public Identity (string id)
        {
            Id = id;
        }

        public string GetMaskedTopic(string topic)
        {
            string[] topicParts = topic.Split('/');
            string[] maskParts = PublishMask.Split('/');

            if (topicParts.Length != 4)
            {
                throw new ArgumentException(nameof(topic));
            }

            for (int i = 0; i < topicParts.Length; i++)
            {
                topicParts[i] = maskParts[i] == "+" ? topicParts[i] : maskParts[i];
            }
            return string.Join('/', topicParts);
        }
    }
}
