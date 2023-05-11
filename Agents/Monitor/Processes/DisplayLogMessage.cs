using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents
{
    internal class DisplayLogMessage : DisplayLogMessageExecute, IProcess
    {
        public string Id { get; set; } = "display_log_message";
        public string? MemberId { get; set; }

        public Task<Assessment> Assess(InformationAdapter information)
        {
            information.Assessment.Result = AssessmentResult.EXECUTE;
            return Task.FromResult(information.Assessment);
        }

        public Task<List<Information>> Spawn(InformationAdapter information)
        {
            throw new NotImplementedException();
        }
    }
}
