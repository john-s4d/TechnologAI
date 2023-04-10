using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;

namespace Technologai
{
    public class ContextId : IComparable
    {
        private string _id;
        private ulong _unixTimestamp;
        private ulong _random;

        internal SortedList<ContextId, ContextId> Related { get; } = new SortedList<ContextId, ContextId>();

        private ContextId()
            : this(ContextProvider.GetTimestampTicks(), ContextProvider.GetRandomUlong()) { }

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

        public int CompareTo(object? obj)
        {
            var context = obj as ContextId;

            var result = _unixTimestamp.CompareTo(context?._unixTimestamp);

            if (result == 0)
            {
                return _random.CompareTo(context?._random);
            }
            return result;
        }

        public override string ToString()
        {
            return _id;
        }
    }
}