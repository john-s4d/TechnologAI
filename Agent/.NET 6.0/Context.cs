using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using QuikGraph;

namespace Technologai
{
    public class Context //<T> : BidirectionalGraph<string?, T> where T : IEdge<string?>
    {
        private readonly Dictionary<string, Information> _library = new();
        private readonly Dictionary<string, Neuron> _neurons = new();
        private readonly Dictionary<string, List<string>> _forwardContext = new();
        private readonly Dictionary<string, List<string>> _reverseContext = new();
        private readonly Dictionary<string, string> _lineage = new();

        public Context(Identity identity) { }

        public void Spawn(string forwardId, string reverseId)
        {
            AddForward(forwardId, reverseId);
            AddReverse(forwardId, reverseId);
            _lineage[forwardId] = reverseId;
        }

        public void Add(InformationAdapter information)
        {
            _library[information.Id] = information;
            //_neurons[information.NeuronId] = information.Neuron;
        }

        public void Add(Information information)
        {
            _library[information.Id] = information;
        }

        public void AddReverse(string forwardId, string reverseId)
        {
            if (!_reverseContext.ContainsKey(forwardId))
            {
                _reverseContext.Add(forwardId, new List<string>());
            }
            if (!_reverseContext[forwardId].Contains(reverseId))
            {
                _reverseContext[forwardId].Add(reverseId);
            }
        }

        public void AddForward(string forwardId, string reverseId)
        {

            if (!_forwardContext.ContainsKey(reverseId))
            {
                _forwardContext.Add(reverseId, new List<string>());
            }
            if (!_forwardContext[reverseId].Contains(forwardId))
            {
                _forwardContext[reverseId].Add(forwardId);
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

        public List<Information> GetForward(string reverseId)
        {
            return _forwardContext.ContainsKey(reverseId) ? ToList(_forwardContext[reverseId]) : new();

        }

        public List<Information> GetReverse(string forwardId)
        {
            return _reverseContext.ContainsKey(forwardId) ? ToList(_reverseContext[forwardId]) : new();
        }

        public Information? GetCreator(string forwardId)
        {
            return _lineage.ContainsKey(forwardId) ? _library[_lineage[forwardId]] : null;
        }

        public string Summarize(string contextId)
        {
            var currentInfo = _library[contextId];
            string summary = $"{currentInfo.InputText} {currentInfo.NeuronId} {currentInfo.OutputText}\n";

            foreach (Information information in GetReverse(contextId))
            {
                summary += $"{information.InputText} {information.NeuronId} {information.OutputText}\n"; // TODO: Neuron Description
            }
            foreach (Information information in GetForward(contextId))
            {
                summary += $"{information.InputText} {information.NeuronId} {information.OutputText} \n"; // TODO: Neuron Description
            }
            return summary;
        }
    }
}

