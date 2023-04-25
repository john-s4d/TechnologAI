using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents.Core.Monitor
{
    internal class DisplayLogMessageExecute : IExecute
    {
        internal event Action<string>? LogMessage;
        public string Description => "Show a message on the output log screen.";

        public string SampleJsonIn => "{\"message\":\"string\"}";

        public string SampleJsonOut => string.Empty;

        public Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            LogMessage?.Invoke((string)data["message"]);

            return Task.FromResult(data);
        }
    }
}
