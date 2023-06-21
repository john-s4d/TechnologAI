namespace Technologai
{
    public class InformationAdapter : Information
    {
        private TechnologaiAgent _agent;
        private Process _process;
        
        //public ContextAdapter Context => _agent.Context;
        //public TechnologaiAgent Agent => _agent;
        //public IProcess Process => _process;

        private InformationAdapter(string id, string creatorId, string workerId, string processId, InformationState informationState, ProcessState processState, string? input = null, string? output = null)
            : base(id, creatorId, workerId, processId, informationState, processState, input, output) { }

        
        public static InformationAdapter Create(TechnologaiAgent agent, Information information)
        {
            return new InformationAdapter(
                information.Id,
                information.CreatorId,
                information.WorkerId,
                information.ProcessId,
                information.InformationState,
                information.ProcessState,
                information.InputText,
                information.OutputText                                                                                                                         )
            {
                _agent = agent,
                _process = agent.Processes[information.ProcessId]
            };
            // TODO: Do we need to add this to the context?
        }

        public async static Task<InformationAdapter> Create(TechnologaiAgent agent, Process process, string? input = null)
        {   
            var information = Create(agent, Create(agent.Identity.Id, process.Id, input));

            agent.Context.Add(information);

            information.WorkerId = process.MemberId ?? agent.Identity.Id;

            await agent.SendStatusMessage($"{information.Id} Create> {process.Id} | {information.InputText}");
            return information;
        }

        protected internal async Task<ProcessState> Assess()
        {
            await _agent.SendStatusMessage($"{Id} Assess> {ProcessId} | {InputText} | {OutputText}");

            this.ProcessState = await _process.Assess(this);

            return this.ProcessState;
        }

        protected internal async Task Execute()
        {
            await _agent.SendStatusMessage($"{Id} Execute> {ProcessId} | {InputText} | {OutputText}");

            var output = await _process.Execute(this);

            if (output is string)
            {
                OutputText = (string)output;
            }

            if (output is Dictionary<string,string>)
            {
                OutputData = (Dictionary<string, string>)output;
            }

            InformationState = InformationState.CLOSED;
            WorkerId = CreatorId;

            await Publish();
        }

        protected internal async Task Spawn()
        {
            await _agent.SendStatusMessage($"{Id} Spawn> {ProcessId} | {InputText} | {OutputText}");

            var items = await _process.Spawn(this); // TODO: clunky

            if (items != null)
            {
                foreach (var item in items)
                {
                    await (Create(_agent, item).Publish());
                }
            }
        }

        public async Task<InformationAdapter> Spawn(string processId, string? input = null)
        {
            //var information = Create(_agent, Create(agent.Identity.Id, process.Id, input));
            var information = await Create(_agent, _agent.Processes[processId], input);
            _agent.Context.Spawn(information.Id, this.Id);
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

        public object? this[string key]
        {
            get
            {
                return null;
            }
            set { }
        }
    }
}
