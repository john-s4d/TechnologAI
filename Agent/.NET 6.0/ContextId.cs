using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace Technologai
{
    public struct ContextId : IComparable<ContextId>
    {
        // hash compute of an id allows to verify which party created this contextId. If that's ever needed.

        private readonly string _id;
        private readonly byte[] _unixTimestampBytes = new byte[8];
        private readonly byte[] _hashComputeBytes = new byte[8];

        public ContextId()
        {
            throw new InvalidOperationException();
        }

        internal ContextId(ulong unixTimestamp, byte[] idHash)
        {
            _unixTimestampBytes = BitConverter.GetBytes(unixTimestamp);
            _hashComputeBytes = MD5.HashData(idHash.Concat(_unixTimestampBytes).ToArray());
            _id = Base64UrlEncoder.Encode(_unixTimestampBytes.Concat(_hashComputeBytes).ToArray());
        }

        internal ContextId(string contextId)
        {
            _id = contextId;
            var contextBytes = Base64UrlEncoder.DecodeBytes(_id);

            Array.Copy(contextBytes, _unixTimestampBytes, 8);
            Array.Copy(contextBytes, 8, _hashComputeBytes, 0, 8);
        }

        internal static ContextId Create(string creatorIdBase64)        {

            return new ContextId(GetTimestampTicksBytes(), GetBase64Bytes(creatorIdBase64, 8));
        }

        public static ulong GetTimestampTicksBytes()
        {
            return (ulong)(DateTimeOffset.UnixEpoch - DateTimeOffset.UtcNow).Ticks;
        }

        public static byte[] GetBase64Bytes(string creatorIdBase64, int count)
        {
            return Base64UrlEncoder.DecodeBytes(creatorIdBase64).Take(count).ToArray();
        }

        public int CompareTo(ContextId other)
        {
            var result = BitConverter.ToInt64(_unixTimestampBytes).CompareTo(
                BitConverter.ToInt64(other._unixTimestampBytes)
            );

            if (result == 0)
            {
                return BitConverter.ToInt64(_hashComputeBytes).CompareTo(
                    BitConverter.ToInt64(other._hashComputeBytes)
                );
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