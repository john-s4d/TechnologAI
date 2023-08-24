namespace Technologai.Templates
{
    public class KillSwitch : Template
    {
        private const int KILL_DELAY = 1000;
        private readonly Agent _agent;

        public KillSwitch(Agent agent)
        {
            Id = "kill_switch";
            Description = "Immediately Shut Down All Agents.";
            InputKeys = new string[] { "initiatorId", "reason" };

            _agent = agent;
        }

        public override Task<bool> Assess(Information information) => Task.FromResult(true);
        
        public override Task<Data?> Process(Information information)
        {
            // FIXME: Only kills Core agent. Need to kill all agents.
            // TODO: Broadcast a message to all agents to shut down.            
            _agent.Kill(KILL_DELAY);
            return Task.FromResult((Data?)null);
        }       
    }
}
