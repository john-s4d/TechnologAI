using Microsoft.VisualBasic;
using System.ComponentModel;
using System.IO;

namespace Technologai
{
    public class ContextProvider
    {
        private readonly Dictionary<ContextId, List<ContextId>> _forward = new(); // reverseId, [forward]
        private readonly Dictionary<ContextId, List<ContextId>> _reverse = new(); // forwardId, [reverse]
        private readonly Dictionary<ContextId, Information> _library = new();
        private readonly Dictionary<ContextId, ContextId> _lineage = new();

        public ContextProvider()
        {

        }

        internal void Add(Information information)
        {
            _library[information.Id] = information;
        }

        internal void Add(ContextId forwardId, ContextId reverseId)
        {
            AddForward(forwardId, reverseId);
            AddReverse(forwardId, reverseId);
        }

        private void AddReverse(ContextId forwardId, ContextId reverseId)
        {
            if (!_reverse.ContainsKey(forwardId))
            {
                _reverse.Add(forwardId, new List<ContextId>());
            }
            if (!_reverse[forwardId].Contains(reverseId))
            {
                _reverse[forwardId].Add(reverseId);
            }
        }

        private void AddForward(ContextId forwardId, ContextId reverseId)
        {

            if (!_forward.ContainsKey(reverseId))
            {
                _forward.Add(reverseId, new List<ContextId>());
            }
            if (!_forward[reverseId].Contains(forwardId))
            {
                _forward[reverseId].Add(forwardId);
            }
        }

        internal List<Information> ToList(List<ContextId> contextIds)
        {
            List<Information> result = new List<Information>();
            foreach (ContextId contextId in contextIds)
            {
                result.Add(_library[contextId]);
            }
            return result;
        }

        internal List<Information>? GetForward(ContextId reverseId)
        {
            return _forward.ContainsKey(reverseId) ? ToList(_forward[reverseId]) : null;

        }
        internal List<Information>? GetReverse(ContextId forwardId)
        {
            return _reverse.ContainsKey(forwardId) ? ToList(_reverse[forwardId]) : null;
        }

        internal Information? GetCreator(ContextId forwardId)
        {
            return _lineage.ContainsKey(forwardId) ? _library[_lineage[forwardId]] : null;
        }

        internal void Spawn(ContextId forwardId, ContextId reverseId)
        {
            _lineage[forwardId] = reverseId;
        }
    }
}