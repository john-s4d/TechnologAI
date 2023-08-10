namespace Technologai
{
    public class InformationAdapter : Information
    {
        private Agent _agent;
        private Template _template;

        //private int SpawnCount { get; set; } = 0;
        //private int ExecuteCount { get; set; } = 0;

        //public ContextAdapter Context => _agent.Context;
        //public TechnologaiAgent Agent => _agent;
        //public Template Template => _template;

        private InformationAdapter(
            string id,
            string creatorId,
            string workerId,
            string templateId,
            InformationState informationState,
            TemplateState templateState,
            string? input = null,
            string? output = null
        )
            : base(
                  id,
                  creatorId,
                  workerId,
                  templateId,
                  informationState,
                  templateState,
                  input,
                  output
            )
        { }

        public static InformationAdapter Create(Agent agent, Information information)
        {
            return new InformationAdapter(
                information.Id,
                information.CreatorId,
                information.WorkerId,
                information.TemplateId,
                information.InformationState,
                information.TemplateState,
                information.InputText,
                information.OutputText)
            {
                _agent = agent,
                _template = agent.Catalog[information.TemplateId]
            };
        }

        public async static Task<InformationAdapter> Create(Agent agent, Template template, string? input = null)
        {
            var information = Create(agent, Create(agent.Identity.Id, template.Id, input));

            agent.Context.Add(information);

            information.WorkerId = template.MemberId ?? agent.Identity.Id;

            //await agent.SendStatusMessage($"{information.Id} Create> {template.Id} | {information.InputText}");
            return information;
        }

        private bool _assessmentQueued = false;

        // Assessments are debounced. Only one assessment can be queued at a time.

        protected internal async Task<bool> Assess()
        {
            if (TemplateState != TemplateState.RESTING && !_assessmentQueued)
            {
                _assessmentQueued = true;

                while (TemplateState != TemplateState.RESTING)
                {
                    await Task.Delay(100);
                }
                
                _assessmentQueued = false;
            }

            if (TemplateState == TemplateState.RESTING)
            {
                TemplateState = TemplateState.ASSESSING;

                //await _agent.SendStatusMessage($"{Id} Assess> {TemplateId} | {InputText} | {OutputText}");

                var result = await _template.Assess(this);

                TemplateState = TemplateState.RESTING;                

                return result;
            }

            return false;            
        }

        // Only one spike can be in progress at a time. We don't queue up another one

        protected internal async Task Process()
        {
            // TODO: This isn't fully threadsafe. Should lock.

            if (TemplateState == TemplateState.RESTING) 
            {
                TemplateState = TemplateState.PROCESSING;
                
                //await _agent.SendStatusMessage($"{Id} Action> {TemplateId} | {InputText} | {OutputText}");

                Output = await _template.Process(this) ?? Output;

                InformationState = InformationState.CLOSED;
                WorkerId = CreatorId;

                TemplateState = TemplateState.RESTING; // Always return to resting.

                await Publish();
            }
        }       

        public async Task<InformationAdapter> Spawn(string templateId, string? input = null)
        {
            var information = await Create(_agent, _agent.Catalog[templateId], input);
            _agent.Context.Spawn(information.Id, this.Id);
            information.WorkerId = _template.MemberId ?? _agent.Identity.Id;
            //_agent.SendStatusMessage($"{information.Id} Spawn> {templateId} | {information.Input}");
            return information;
        }

        public async Task Publish(Agent.PublishCallback? publishCallback = null)
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
