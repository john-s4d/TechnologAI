using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public abstract class Identity
    {
        internal List<string> AssignedRoles { get; } = new List<string>();
        internal string? Name { get; set; }
        public string? Id { get; set; }
        internal abstract string RoleName { get; }
        internal abstract string PublishMask { get; }
        internal abstract string SubscribeMask { get; }

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
