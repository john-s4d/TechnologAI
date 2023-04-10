using System.Security.Cryptography;

namespace Technologai
{
    public class ContextProvider //: List<ContextId>
    {

        private Dictionary<string, string> _contextOwners = new Dictionary<string, string>();
        private Dictionary<string, string> _contextHierarchy = new Dictionary<string, string>();

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

        internal void RecordContext(Information information)
        {
            if (information.OwnerId == null)
            {
                throw new ArgumentNullException(nameof(information.OwnerId));
            }
            if (information.ContextId == null)
            {
                throw new ArgumentNullException(nameof(information.ContextId));
            }

            if (!_contextOwners.ContainsKey(information.ContextId))
            {
                _contextOwners.Add(information.ContextId, information.OwnerId);
            }
        }

        internal void NewContext(Information information)
        {
            if (information.ContextId != null)
            {
                string newContextId = Create().ToString();
                _contextHierarchy.Add(newContextId, information.ContextId);
                information.ContextId = newContextId;
            }
            else
            {
                information.ContextId = Create().ToString();
                information.OwnerId = _identity?.Id ?? throw new ArgumentNullException(nameof(_identity));
            }            
        }

        internal void SetContextOrOwner(Information information)
        {
            if (information.ContextId == null)
            {
                NewContext(information);
                
            }
            else
            {       /*                     
                if (_identity.AssignedRole == "agency")
                {
                    if (_contextHierarchy.ContainsKey(information.ContextId))
                    {
                        information.ContextId = _contextHierarchy[information.ContextId];
                    }
                }
                else if (_identity.AssignedRole == "member")
                {
                    if(_contextOwners.ContainsKey(information.ContextId))
                    {
                        information.OwnerId = _contextOwners[information.ContextId];
                    }
                    
                }  */           
            }
            
        }
    }
}