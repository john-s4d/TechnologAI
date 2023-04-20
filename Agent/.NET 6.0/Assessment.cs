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
        public string? Content { get; set; }       
    }
}
