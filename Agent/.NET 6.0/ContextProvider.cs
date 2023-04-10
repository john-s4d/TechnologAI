using System.Security.Cryptography;

namespace Technologai
{
    public class ContextProvider
    {
        private Dictionary<string, string> _contextOwners = new();
        private Dictionary<string, string> _contextHierarchy = new();

        private Identity _identity;

        public ContextProvider(Identity identity)
        {
            _identity = identity;
        }

        public static ContextId Create()
        {
            return new ContextId(GetTimestampTicks(), GetRandomUlong());
        }

        public static ulong GetTimestampTicks()
        {
            return (ulong)(DateTimeOffset.UnixEpoch - DateTimeOffset.UtcNow).Ticks;
        }

        public static ulong GetRandomUlong()
        {
            return BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8));
        }
      
        internal Information Spawn(Information information)
        {
            string contextId = Create().ToString();

            if (information.ContextId != null)
            {
                _contextHierarchy.Add(contextId, information.ContextId);
            }

            return new Information()
            {
                ContextId = contextId,
                OwnerId = _identity.Id
            };
        }

        internal void OnReceive(Information information)
        {
            if (_identity.AssignedRole == "member" && information.ContextId != null)
            {
                if (_contextHierarchy.ContainsKey(information.ContextId))
                {
                    information.ContextId = _contextHierarchy[information.ContextId];
                }
            }
            if (_identity.AssignedRole == "agency" && information.ContextId != null)
            {
                if (!_contextOwners.ContainsKey(information.ContextId) && information.OwnerId != null)
                {
                    _contextOwners.Add(information.ContextId, information.OwnerId);
                }
                else if (_contextOwners.ContainsKey(information.ContextId))
                {
                    information.OwnerId = _contextOwners[information.ContextId];
                }
                // TODO: Clean up the cache every now and then
            }
        }


        internal void OnPublish(Information information)
        {
            if (_identity.AssignedRole == "member")
            {
                if (information.ContextId == null)
                {
                    information.ContextId = Create().ToString();
                }
                if (information.OwnerId == null)
                {
                    information.OwnerId = _identity?.Id ?? throw new ArgumentNullException(nameof(_identity));
                }
            }
            if (_identity.AssignedRole == "agency")
            {

            }
        }
    }
}