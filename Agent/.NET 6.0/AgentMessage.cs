namespace Technologai
{
    public enum AgentMessageType
    {
        HELLO,        
        PROCESS,        
        INFORMATION,
        CONTEXT
    }

    public class AgentMessage
    {
        public AgentMessageType? Type { get; set; }
        public object? Data { get; set; }

    }
}
