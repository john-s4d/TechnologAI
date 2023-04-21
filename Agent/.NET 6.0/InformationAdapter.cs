using Newtonsoft.Json;

namespace Technologai
{
    public class InformationAdapter
    {
        private TechnologaiAgent _agent;
        private IAbility _ability;
        private Information _information;
        private Context<Information> _catalog;

        // Agent
        public string AgentId => _agent.Identity.Id;
        public string WorkerId { get; set; }

        // Context
        public Context<Information> Catalog => _catalog;
        public TechnologaiAgent Agent => _agent;
        public Information Information => _information; 
        public IAbility Ability => _ability;    

        // Information
        public string ContextId => _information.Id;
        public string CreatorId => _information.CreatorId;
        public InformationState State => _information.State;
        public string? Input => _information.Input;
        public string? Output => _information.Output;
        public string? Assessment { get; set; }

        // Ability
        public string AbilityId => _ability.Id;
        public string? Description => _ability.Description;
        public string? SampleJsonIn => _ability.SampleJsonIn;
        public string? SampleJsonOut => _ability.SampleJsonOut;
        public string? Prompt => _ability.Prompt;

        public InformationAdapter(TechnologaiAgent agent, IAbility ability, Information information)
        {
            _agent = agent;
            _ability = ability;
            _information = information;
            _context = _agent.Context.RelatedTo(information.Id);
            WorkerId = _ability.MemberId ?? _agent.Identity.Id;
        }

        public InformationAdapter(TechnologaiAgent agent, Information information)
        {
            _agent = agent;
            _ability = _agent.Abilities[information.AbilityId];
            _information = information;
            _context = _agent.Context.RelatedTo(information.Id);
            WorkerId = _ability.MemberId ?? _agent.Identity.Id;
        }

        public static InformationAdapter Create(TechnologaiAgent agent, string abilityId, string? input = null)
        {
            return Create(agent, agent.Abilities[abilityId], input);
        }

        public static InformationAdapter Create(TechnologaiAgent agent, IAbility ability, string? input = null)
        {
            var information = Information.Create(agent.Identity.Id, ability.Id, input);
            agent.Context.Add(information);

            var adapter = new InformationAdapter(agent, ability, information);
            adapter.WorkerId = ability.MemberId ?? agent.Identity.Id;

            agent.SendStatusMessage($"{information.Id} Create> {ability.Id} | {information.Input}");
            return adapter;
        }

        protected internal async Task<AssessmentResult> Assess()
        {
            _agent.SendStatusMessage($"{ContextId} Assess> {AbilityId} | {Input} | {Output}");
            return await _ability.Assess(this);
        }

        protected internal async Task Execute()
        {
            _agent.SendStatusMessage($"{ContextId} Execute> {AbilityId} | {Input} | {Output}");
            _information.Output = await _ability.Execute(this);
            _information.State = InformationState.CLOSED;
            WorkerId = CreatorId;
            await Publish();
        }

        protected internal async Task Spawn()
        {
            _agent.SendStatusMessage($"{ContextId} Spawn> {AbilityId} | {Input} | {Output}");

            foreach (Information item in await _ability.Spawn(this))
            {
                await (new InformationAdapter(_agent, item).Publish());
            }
        }

        public InformationAdapter GetSpawn(string abilityId, string? input = null)
        {
            var information = Create(_agent, abilityId, input ?? Input);
            _agent.Context.Spawn(information.ContextId, _information.Id);
            information.WorkerId = _ability.MemberId ?? _agent.Identity.Id;
            //_agent.SendStatusMessage($"{information.Id} Spawn> {abilityId} | {information.Input}");
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
