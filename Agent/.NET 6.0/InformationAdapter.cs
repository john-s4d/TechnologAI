namespace Technologai
{
    public class InformationAdapter : Information
    {
        private Agent _agent;
        private Template _template;

        private InformationAdapter(
            string id,
            string creatorId,
            string workerId,
            string templateId,
            InformationState informationState,
            TemplateState templateState,
            Data? input = null,
            Data? output = null
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
                information.Input, 
                information.Output)
            {
                _agent = agent,
                _template = (Template)agent.Catalog[information.TemplateId]
            };
        }

        public async static Task<InformationAdapter> Create(Agent agent, Template template, Data? input = null)
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

        //TODO: Spawn and Publish Immediately
        public async Task<InformationAdapter> Spawn(string? templateId, Data? input = null)
        {
            var information = await Create(_agent, (Template)_agent.Catalog[templateId], input);
            _agent.Context.Spawn(information.Id, this.Id);            
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
