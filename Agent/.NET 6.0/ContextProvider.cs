using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Technologai
{
    public class ContextProvider
    {   
        private Dictionary<ContextId, ContextId> _contextHierarchy = new();
        private Dictionary<ContextId, Information> _completedInformation = new();

        private Identity _identity;

        public ContextProvider(Identity identity)
        {
            _identity = identity;
        }

        public ContextId CreateContextId()
        {
            return new ContextId(GetTimestampTicksBytes(), GetIdentityBytes(8));
        }

        internal Information CreateInformation(string input)
        {   
            var information = new Information(CreateContextId(), _identity.Id ?? 
                throw new ArgumentNullException(nameof(_identity.Id)));
            information.Input = input;

            return information;
        }

        internal Information CreateInformation()            
        {
            return CreateInformation(string.Empty);
        }

        public static ulong GetTimestampTicksBytes()
        {
            return (ulong)(DateTimeOffset.UnixEpoch - DateTimeOffset.UtcNow).Ticks;
        }

        public byte[] GetIdentityBytes(int count)
        {
            return Base64UrlEncoder.DecodeBytes(_identity.Id).Take(count).ToArray();
        }

        internal Information Spawn(Information information, InformationState state = InformationState.OPEN, string? input = null)
        {
            string contextId = CreateContextId();
            
            _contextHierarchy.Add(contextId, information.ContextId);

            var information_new = new Information(contextId, _identity.Id ?? throw new ArgumentNullException(nameof(_identity.Id)));
            information_new.Input = string.IsNullOrEmpty(input) ? null : input;
            return information_new;
        }

       internal void MarkComplete(Information information) {

            if (!_completedInformation.ContainsKey(information.ContextId))
            {
                _completedInformation.Add(information.ContextId, information);
            }
        }
    }
}