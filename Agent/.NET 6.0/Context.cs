using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;

namespace Technologai
{
    public class Context : IComparable
    {
        public string Id { get; }
        public ulong UnixTimestamp { get; }
        public ulong Random { get; }

        private Context()
            : this(GetTimestampTicks(), GetRandomUlong()) { }

        public Context(ulong unixTimestamp, ulong random)
        {
            UnixTimestamp = unixTimestamp;
            Random = random;

            var timestampBytes = BitConverter.GetBytes(UnixTimestamp);
            var randomBytes = BitConverter.GetBytes(UnixTimestamp);

            Id = Base64UrlEncoder.Encode(timestampBytes.Concat(randomBytes).ToArray());
        }

        public Context(string contextId)
        {
            Id = contextId;

            var contextBytes = Base64UrlEncoder.DecodeBytes(Id);

            byte[] timestampBytes = new byte[8];
            byte[] randomBytes = new byte[8];

            Array.Copy(contextBytes, timestampBytes, 8);
            Array.Copy(contextBytes, 8, randomBytes, 0, 8);

            UnixTimestamp = BitConverter.ToUInt64(timestampBytes);
            Random = BitConverter.ToUInt64(randomBytes);
        }

        public static Context Create()
        {
            return new Context();
        }

        public static ulong GetTimestampTicks()
        {
            return (ulong)(DateTimeOffset.UnixEpoch - DateTimeOffset.UtcNow).Ticks;
        }

        public static ulong GetRandomUlong()
        {
            return BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8));
        }

        public override string ToString()
        {
            return Id;
        }

        public int CompareTo(object? obj)
        {
            var context = obj as Context;

            var result = UnixTimestamp.CompareTo(context?.UnixTimestamp);

            if (result == 0)
            {
                return Random.CompareTo(context?.Random);
            }
            return result;
        }
    }
}