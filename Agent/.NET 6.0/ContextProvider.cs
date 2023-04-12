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

        internal Information CreateInformation(string input)
        {
            var creatorId = _identity.Id ?? throw new ArgumentNullException(nameof(_identity.Id));
            return new Information(creatorId, input);           
        }

        internal Information Spawn(Information information, string input)
        {
            var information_new = CreateInformation(input);            

            _contextHierarchy.Add(information_new.ContextId, information.ContextId);

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