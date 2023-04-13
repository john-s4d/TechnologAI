using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Technologai
{
    public class ContextProvider
    {   
        private Dictionary<string, List<string>> _contextHierarchy = new();
        private Dictionary<string, Information> _completedInformation = new();

        private Identity _identity;

        public ContextProvider(Identity identity)
        {
            _identity = identity;
        }

        internal void Link(Information newInformation, Information oldInformation)
        {
            if (!_contextHierarchy.ContainsKey(newInformation.ContextId))
            {
                _contextHierarchy.Add(newInformation.ContextId, new List<string>());
            }
            _contextHierarchy[newInformation.ContextId].Add(oldInformation.ContextId);
        }
    }
}