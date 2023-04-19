using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public enum AssessmentResult
    {
        SPAWN,
        EXECUTE,
        HOLD
    }

    public class Assessment
    {
        public AssessmentResult Result { get; set; } = AssessmentResult.HOLD;
        public string? ForwardSummary { get; set; }
        public string? ReverseSummary { get; set; }
        public List<Information>? ForwardContext { get; set; }
        public List<Information>? ReverseContext { get; set; }
        public Assessment(List<Information>? forwardContext, List<Information>? reverseContext)
        {
        }
    }
}
