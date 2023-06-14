using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace Technologai
{
    public class InformationId : IComparable<InformationId>
    {
        // hash compute of an id allows to verify which party created this contextId. If that's ever needed.
                
        private readonly string _id;
        private readonly byte[] _unixTimestampBytes = new byte[8];
        private readonly byte[] _hashComputeBytes = new byte[8];

        internal InformationId(ulong unixTimestamp, byte[] idHash)
        {
            _unixTimestampBytes = BitConverter.GetBytes(unixTimestamp);
            _hashComputeBytes = MD5.HashData(idHash.Concat(_unixTimestampBytes).ToArray()).Take(8).ToArray(); // just half of the hash to fit into 8 bytes
            _id = Base64UrlEncoder.Encode(_unixTimestampBytes.Concat(_hashComputeBytes).ToArray());
        }

        [JsonConstructor]
        public InformationId(string contextId)
        {
            _id = contextId;
            var contextBytes = Base64UrlEncoder.DecodeBytes(_id);

            Array.Copy(contextBytes, _unixTimestampBytes, 8);
            Array.Copy(contextBytes, 8, _hashComputeBytes, 0, 8);
        }

        internal static InformationId Create(string creatorIdBase64)        {

            return new InformationId(GetTimestampTicksBytes(), GetBase64Bytes(creatorIdBase64, 8));
        }

        public static ulong GetTimestampTicksBytes()
        {
            return (ulong)(DateTimeOffset.UnixEpoch - DateTimeOffset.UtcNow).Ticks;
        }

        public static byte[] GetBase64Bytes(string creatorIdBase64, int count)
        {
            return Base64UrlEncoder.DecodeBytes(creatorIdBase64).Take(count).ToArray();
        }

        public int CompareTo(InformationId? other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

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

        public static implicit operator InformationId(string value) => new InformationId(value);

        public static implicit operator string(InformationId value) => value.ToString();       

        public override string ToString()
        {
            return _id;
        }
    }
}