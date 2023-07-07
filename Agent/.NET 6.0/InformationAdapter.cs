namespace Technologai
{
    public class InformationAdapter : Information
    {
        private TechnologaiAgent _agent;
        private Neuron _neuron;

        //private int SpawnCount { get; set; } = 0;
        //private int ExecuteCount { get; set; } = 0;

        //public ContextAdapter Context => _agent.Context;
        //public TechnologaiAgent Agent => _agent;
        //public IProcess Process => _process;

        private InformationAdapter(
            string id,
            string creatorId,
            string workerId,
            string processId,
            InformationState informationState,
            string? input = null,
            string? output = null
        )
            : base(
                  id,
                  creatorId,
                  workerId,
                  processId,
                  informationState,
                  input,
                  output
            )
        { }

        public static InformationAdapter Create(TechnologaiAgent agent, Information information)
        {
            return new InformationAdapter(
                information.Id,
                information.CreatorId,
                information.WorkerId,
                information.NeuronId,
                information.InformationState,
                information.InputText,
                information.OutputText)
            {
                _agent = agent,
                _neuron = agent.Neurons[information.NeuronId]
            };
        }

        public async static Task<InformationAdapter> Create(TechnologaiAgent agent, Neuron neuron, string? input = null)
        {
            var information = Create(agent, Create(agent.Identity.Id, neuron.Id, input));

            agent.Context.Add(information);

            information.WorkerId = neuron.MemberId ?? agent.Identity.Id;

            await agent.SendStatusMessage($"{information.Id} Create> {neuron.Id} | {information.InputText}");
            return information;
        }

        protected internal async Task<bool> Assess()
        {
            await _agent.SendStatusMessage($"{Id} Assess> {NeuronId} | {InputText} | {OutputText}");

            return await _neuron.Assess(this);
        }

        protected internal async Task Spike()
        {
            await _agent.SendStatusMessage($"{Id} Spike> {NeuronId} | {InputText} | {OutputText}");

            Output = await _neuron.Spike(this) ?? Output;

            InformationState = InformationState.CLOSED;            
            WorkerId = CreatorId;

            await Publish();           
        }       

        public async Task<InformationAdapter> Spawn(string processId, string? input = null)
        {
            var information = await Create(_agent, _agent.Neurons[processId], input);
            _agent.Context.Spawn(information.Id, this.Id);
            information.WorkerId = _neuron.MemberId ?? _agent.Identity.Id;
            //_agent.SendStatusMessage($"{information.Id} Spawn> {processId} | {information.Input}");
            return information;
        }

        public async Task Publish(TechnologaiAgent.OnPublished? onPublished = null)
        {
            await _agent.Publish(this, onPublished);            
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
