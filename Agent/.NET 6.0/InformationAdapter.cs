using Newtonsoft.Json;

namespace Technologai
{
    public class InformationAdapter
    {
        private TechnologaiAgent _agent;
        private IProcess _process;
        private Information _information;

        // Agent
        public string AgentId => _agent.Identity.Id;
        public string WorkerId { get; set; }

        // Context
        public TechnologaiAgent Agent => _agent;
        public Information Information => _information;
        public IProcess Process => _process;

        public ContextAdapter Context
        {
            get { return _agent.Context; }
        }

        // Information
        public string ContextId => _information.Id;
        public string CreatorId => _information.CreatorId;
        public InformationState State => _information.State;
        public string? Input => _information.Input;
        public string? Output => _information.Output;
        public Assessment Assessment { get; set; } = new();

        public string ProcessId => _process.Id;

        public InformationAdapter(TechnologaiAgent agent, IProcess process, Information information)
        {
            _agent = agent;
            _process = process;
            _information = information;
            //_context = _agent.Context.Neighbors(information.Id);
            WorkerId = _process.MemberId ?? _agent.Identity.Id;
        }

        public InformationAdapter(TechnologaiAgent agent, Information information)
        {
            _agent = agent;
            _process = _agent.Processes[information.ProcessId];
            _information = information;
            //_context = _agent.Context.RelatedTo(information.Id);
            WorkerId = _process.MemberId ?? _agent.Identity.Id;
        }

        public static InformationAdapter? Create(TechnologaiAgent agent, string processId, string? input = null)
        {
            return Create(agent, agent.Processes[processId], input).Result;
        }

        public async static Task<InformationAdapter> Create(TechnologaiAgent agent, IProcess process, string? input = null)
        {
            var information = Information.Create(agent.Identity.Id, process.Id, input);
            agent.Context.Add(information);

            var adapter = new InformationAdapter(agent, process, information);
            adapter.WorkerId = process.MemberId ?? agent.Identity.Id;

            await agent.SendStatusMessage($"{information.Id} Create> {process.Id} | {information.Input}");
            return adapter;
        }

        public async Task<string> Summarize()
        {
            await _agent.SendStatusMessage($"{ContextId} Summarize> {ProcessId} | {Input} | {Output}");

            return Context.Summarize(ContextId);
        }

        protected internal async Task<Assessment> Assess()
        {
            await _agent.SendStatusMessage($"{ContextId} Assess> {ProcessId} | {Input} | {Output}");
            return await _process.Assess(this);
        }

        protected internal async Task Execute(Assessment assessment)
        {
            await _agent.SendStatusMessage($"{ContextId} Execute> {ProcessId} | {Input} | {Output}");
            var result = await _process.Execute(assessment.Data);
            _information.Output = JsonConvert.SerializeObject(result, Formatting.None);
            _information.State = InformationState.CLOSED;
            WorkerId = CreatorId;
            await Publish();
        }

        protected internal async Task Spawn(Assessment assessment)
        {
            await _agent.SendStatusMessage($"{ContextId} Spawn> {ProcessId} | {Input} | {Output}");

            foreach (Information item in await _process.Spawn(this))
            {
                await (new InformationAdapter(_agent, item).Publish());
            }
        }

        public InformationAdapter GetSpawn(string processId, string? input = null)
        {
            var information = Create(_agent, processId, input);
            _agent.Context.Spawn(information.ContextId, _information.Id);
            information.WorkerId = _process.MemberId ?? _agent.Identity.Id;
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

        public T? DeserializeInput<T>()
        {
            return JsonConvert.DeserializeObject<T>(Input ?? string.Empty);
        }

        public T? DeserializeOutput<T>()
        {
            return JsonConvert.DeserializeObject<T>(Output ?? string.Empty);
        }


        public static implicit operator Information(InformationAdapter value) => value._information;

    }
}
