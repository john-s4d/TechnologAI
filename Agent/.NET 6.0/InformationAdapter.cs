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
            NeuronState neuronState,
            string? input = null,
            string? output = null
        )
            : base(
                  id,
                  creatorId,
                  workerId,
                  processId,
                  informationState,
                  neuronState,
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
                information.NeuronState,
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

            //await agent.SendStatusMessage($"{information.Id} Create> {neuron.Id} | {information.InputText}");
            return information;
        }

        private bool _assessmentQueued = false;

        // Assessments are debounced. Only one assessment can be queued at a time.

        protected internal async Task<bool> Assess()
        {
            if (NeuronState != NeuronState.RESTING && !_assessmentQueued)
            {
                _assessmentQueued = true;

                while (NeuronState != NeuronState.RESTING)
                {
                    await Task.Delay(100);
                }
                
                _assessmentQueued = false;
            }

            if (NeuronState == NeuronState.RESTING)
            {
                NeuronState = NeuronState.ASSESSING;

                //await _agent.SendStatusMessage($"{Id} Assess> {NeuronId} | {InputText} | {OutputText}");

                var result = await _neuron.Assess(this);

                NeuronState = NeuronState.RESTING;                

                return result;
            }

            return false;            
        }

        // Only one spike can be in progress at a time. We don't queue up another one

        protected internal async Task Spike()
        {
            // TODO: This isn't fully threadsafe. Should lock.

            if (NeuronState == NeuronState.RESTING) 
            {
                NeuronState = NeuronState.SPIKING;
                
                //await _agent.SendStatusMessage($"{Id} Spike> {NeuronId} | {InputText} | {OutputText}");

                Output = await _neuron.Spike(this) ?? Output;

                InformationState = InformationState.CLOSED;
                WorkerId = CreatorId;

                NeuronState = NeuronState.RESTING; // Always return to resting.

                await Publish();
            }
        }       

        public async Task<InformationAdapter> Spawn(string processId, string? input = null)
        {
            var information = await Create(_agent, _agent.Neurons[processId], input);
            _agent.Context.Spawn(information.Id, this.Id);
            information.WorkerId = _neuron.MemberId ?? _agent.Identity.Id;
            //_agent.SendStatusMessage($"{information.Id} Spawn> {processId} | {information.Input}");
            return information;
        }

        public async Task Publish(TechnologaiAgent.PublishCallback? publishCallback = null)
        {
            await _agent.Publish(this, publishCallback);            
        }

        public async Task<Data?> PublishAndWait()
        {
            return await _agent.PublishAndWait(this);
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
