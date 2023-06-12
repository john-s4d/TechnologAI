using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public enum AgentMessageType
    {
        INFORMATION = 0,
        PROCESS = 1,
        CONTEXT = 2
    }

    public class AgentMessage
    {
        public AgentMessageType? Type { get; set; }
        public object? Data { get; set; }

    }
}
