using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using QuikGraph;

namespace Technologai
{
    public class ContextAdapter //<T> : BidirectionalGraph<string?, T> where T : IEdge<string?>
    {
        private readonly Dictionary<string, Information> _library = new();
        //private readonly Dictionary<string, IAbility> _abilities = new();
        private readonly Dictionary<string, List<string>> _forward = new();
        private readonly Dictionary<string, List<string>> _reverse = new();
        private readonly Dictionary<string, string> _lineage = new();

        public ContextAdapter(Identity identity) { }

        public void Spawn(string forwardId, string reverseId)
        {
            AddForward(forwardId, reverseId);
            AddReverse(forwardId, reverseId);
            _lineage[forwardId] = reverseId;
        }

        public void Add(InformationAdapter information)
        {
            _library[information.ContextId] = information;
            //_abilities[information.AbilityId] = information.Ability;
        }

        public void Add(Information information)
        {
            _library[information.Id] = information;
        }

        public void AddReverse(string forwardId, string reverseId)
        {
            if (!_reverse.ContainsKey(forwardId))
            {
                _reverse.Add(forwardId, new List<string>());
            }
            if (!_reverse[forwardId].Contains(reverseId))
            {
                _reverse[forwardId].Add(reverseId);
            }
        }

        public void AddForward(string forwardId, string reverseId)
        {

            if (!_forward.ContainsKey(reverseId))
            {
                _forward.Add(reverseId, new List<string>());
            }
            if (!_forward[reverseId].Contains(forwardId))
            {
                _forward[reverseId].Add(forwardId);
            }
        }

        public List<Information> ToList(List<string> contextIds)
        {
            List<Information> result = new List<Information>();
            foreach (string contextId in contextIds)
            {
                result.Add(_library[contextId]);
            }
            return result;
        }

        public Dictionary<string, Information> MapByAbility(List<string> contextIds)
        {
            Dictionary<string, Information> result = new();

            foreach (string contextId in contextIds)
            {
                var information = _library[contextId];
                result.Add(information.AbilityId, information);
            }
            return result;
        }


        public List<Information> GetForward(string reverseId)
        {
            return _forward.ContainsKey(reverseId) ? ToList(_forward[reverseId]) : new();

        }

        public List<Information> GetReverse(string forwardId)
        {
            return _reverse.ContainsKey(forwardId) ? ToList(_reverse[forwardId]) : new();
        }

        public Information? GetCreator(string forwardId)
        {
            return _lineage.ContainsKey(forwardId) ? _library[_lineage[forwardId]] : null;
        }

        public string Summarize(string contextId)
        {
            var currentInfo = _library[contextId];
            string summary = $"{currentInfo.Input} {currentInfo.AbilityId} {currentInfo.Output}\n";

            foreach (Information information in GetReverse(contextId))
            {
                summary += $"{information.Input} {information.AbilityId} {information.Output}\n"; // TODO: Ability Description
            }
            foreach (Information information in GetForward(contextId))
            {
                summary += $"{information.Input} {information.AbilityId} {information.Output} \n"; // TODO: Ability Description
            }
            return summary;
        }
    }
}

