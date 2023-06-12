using System.Diagnostics;

namespace Technologai
{
    public class InformationAdapter : Information
    {
        private TechnologaiAgent _agent;
        private IProcess _process;

        // Agent
        public string AgentId => _agent.Identity.Id;
        public string WorkerId { get; set; }

        // Context
        public ContextAdapter Context => _agent.Context;
        public TechnologaiAgent Agent => _agent;

        // Process
        public IProcess Process => _process;
        public ProcessState ProcessState => _process.State;

        private InformationAdapter(string id, string creatorId, string processId, InformationState state, string? input = null, string? output = null)
            : base(id, creatorId, processId, state, input, output) { }

        public static InformationAdapter Create(TechnologaiAgent agent, Information information)
        {
            return new InformationAdapter(
                information.Id,
                information.CreatorId,
                information.ProcessId,
                information.State,
                information.Input,
                information.Output
                                                                                                                         )
            {
                _agent = agent,
                _process = agent.Processes[information.ProcessId]
            };
        }


        public static InformationAdapter Create(TechnologaiAgent agent, string processId, string? input = null)
        {
            return Create(agent, agent.Processes[processId], input).Result;
        }

        public async static Task<InformationAdapter> Create(TechnologaiAgent agent, IProcess process, string? input = null)
        {
            var information = InformationAdapter.Create(agent, process.Id, input);
            agent.Context.Add(information);

            information.WorkerId = process.WorkerId ?? agent.Identity.Id;

            await agent.SendStatusMessage($"{information.Id} Create> {process.Id} | {information.Input}");
            return information;
        }

        protected internal async Task<ProcessState> Assess()
        {
            await _agent.SendStatusMessage($"{Id} Assess> {ProcessId} | {Input} | {Output}");
            return _process.Assess(this);
        }

        protected internal async Task Execute()
        {
            await _agent.SendStatusMessage($"{Id} Execute> {ProcessId} | {Input} | {Output}");
            Output = _process.Execute(this);
            State = InformationState.CLOSED;
            WorkerId = CreatorId;
            await Publish();
        }

        protected internal async Task Spawn()
        {
            await _agent.SendStatusMessage($"{Id} Spawn> {ProcessId} | {Input} | {Output}");

            foreach (InformationAdapter item in _process.Spawn(this))
            {
                await (item.Publish());
            }
        }

        public InformationAdapter GetSpawn(string processId, string? input = null)
        {
            var information = Create(_agent, processId, input);
            _agent.Context.Spawn(information.Id, this.Id);
            information.WorkerId = _process.WorkerId ?? _agent.Identity.Id;
            //_agent.SendStatusMessage($"{information.Id} Spawn> {processId} | {information.Input}");
            return information;
        }

        public async Task Publish()
        {
            await _agent.Publish(this);
        }

        public void PublishWithCallback(TechnologaiAgent.OnPublished onPublished)
        {
            _agent.PublishWithCallback(this, onPublished);
        }

        public object? this[string key]
        {
            get
            {
                return null;
            }
            set { }
        }

        /*
        public T? DeserializeInput<T>()
        {
            return JsonConvert.DeserializeObject<T>(Input ?? string.Empty);
        }

        public T? DeserializeOutput<T>()
        {
            return JsonConvert.DeserializeObject<T>(Output ?? string.Empty);
        }*/

        //public static implicit operator Information(InformationAdapter value) => value._information;

    }
}
