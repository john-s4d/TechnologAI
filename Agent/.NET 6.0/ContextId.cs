using Microsoft.IdentityModel.Tokens;

namespace Technologai
{
    public struct ContextId : IComparable<ContextId>
    {
        private readonly string _id;
        private readonly ulong _unixTimestamp;
        private readonly ulong _random;

        internal SortedList<ContextId, ContextId> Related { get; } = new SortedList<ContextId, ContextId>();

        public ContextId(ulong unixTimestamp, ulong random)
        {
            _unixTimestamp = unixTimestamp;
            _random = random;

            var timestampBytes = BitConverter.GetBytes(_unixTimestamp);
            var randomBytes = BitConverter.GetBytes(_random);

            _id = Base64UrlEncoder.Encode(timestampBytes.Concat(randomBytes).ToArray());
        }

        public ContextId(string contextId)
        {
            _id = contextId;

            var contextBytes = Base64UrlEncoder.DecodeBytes(_id);

            byte[] timestampBytes = new byte[8];
            byte[] randomBytes = new byte[8];

            Array.Copy(contextBytes, timestampBytes, 8);
            Array.Copy(contextBytes, 8, randomBytes, 0, 8);

            _unixTimestamp = BitConverter.ToUInt64(timestampBytes);
            _random = BitConverter.ToUInt64(randomBytes);
        }

        public int CompareTo(ContextId other)
        {
            var result = _unixTimestamp.CompareTo(other._unixTimestamp);

            if (result == 0)
            {
                return _random.CompareTo(other._random);
            }
            return result;
        }

        public static implicit operator ContextId(string contextId)
        {
            return new ContextId(contextId);
        }

        public static implicit operator string(ContextId contextId)
        {
            return contextId._id;
        }

        public override string ToString()
        {
            return _id;
        }
    }
}