namespace Technologai
{
    public enum AgentMessageType
    {
        PROCESS_INQUIRY,
        PROCESS,
        //INFORMATION_INQUIRY, // Broadcast
        INFORMATION,
        CONTEXT
    }

    public class AgentMessage
    {
        public AgentMessageType? Type { get; set; }
        public object? Data { get; set; }

    }
}
