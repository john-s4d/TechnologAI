using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class Context
    {
        public  Information Current { get; set; }
        public string? ForwardSummary { get; set; }
        public string? ReverseSummary { get; set; }
        public List<Information>? ForwardContext { get; set; }
        public List<Information>? ReverseContext { get; set; }
        public Context(Information current, List<Information>? forwardContext, List<Information>? reverseContext)
        {
            Current = current;
            ForwardContext = forwardContext;
            ReverseContext = reverseContext;
        }
    }
}
