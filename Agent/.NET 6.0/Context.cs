using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph;

namespace Technologai
{
    public class Context //<T> : BidirectionalGraph<string?, T> where T : IEdge<string?>
    {
        private readonly Dictionary<string, Information> _catalog = new();
        private readonly Dictionary<string, IAbility> _abilities = new();
        private readonly Dictionary<string, List<string>> _forward = new();
        private readonly Dictionary<string, List<string>> _reverse = new();
        private readonly Dictionary<string, string> _lineage = new();

        public Context(Identity identity) { }

        internal void Add(InformationAdapter information)
        {
            _catalog[information.ContextId] = information;
            _abilities[information.AbilityId] = information.Ability;
        }

        internal void Spawn(string forwardId, string reverseId)
        {
            AddForward(forwardId, reverseId);
            AddReverse(forwardId, reverseId);
            _lineage[forwardId] = reverseId;
        }

        internal void Add(Information information)
        {
            _catalog[information.Id] = information;
        }


        private void AddReverse(string forwardId, string reverseId)
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

        private void AddForward(string forwardId, string reverseId)
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

        internal List<Information> ToList(List<string> contextIds)
        {
            List<Information> result = new List<Information>();
            foreach (string contextId in contextIds)
            {
                result.Add(_catalog[contextId]);
            }
            return result;
        }

        internal List<Information>? GetForward(string reverseId)
        {
            return _forward.ContainsKey(reverseId) ? ToList(_forward[reverseId]) : null;

        }
        internal List<Information>? GetReverse(string forwardId)
        {
            return _reverse.ContainsKey(forwardId) ? ToList(_reverse[forwardId]) : null;
        }

        internal Information? GetCreator(string forwardId)
        {
            return _lineage.ContainsKey(forwardId) ? _catalog[_lineage[forwardId]] : null;
        }

        /*
        internal Context RelatedTo(string contextId)
        {
            var context = new Context();
            context.AddVerticesAndEdge(_library[contextId]);

            var forwardEdges = GetForward(contextId);
            if (forwardEdges != null)
            {
                context.AddVerticesAndEdgeRange(forwardEdges);
            }

            var reverseEdges = GetReverse(contextId);
            if (reverseEdges != null)
            {
                context.AddVerticesAndEdgeRange(reverseEdges);

            }
            return context;
        }*/
    }
}

