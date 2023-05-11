using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai.Agents
{
    internal class DisplayLogMessageExecute : IExecute
    {
        internal event Action<string>? LogMessage;
        public string Description => "Show a message on the output log screen.";

        public string SampleJsonIn => "{\"message\":\"string\",\"count\":\"string\"}";
        

        public string SampleJsonOut => string.Empty;

        public Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            int count = int.Parse((string)data["count"]);

            LogMessage?.Invoke((string)data["message"]);

            return Task.FromResult(new Dictionary<string, object>());
            
        }
    }
}
