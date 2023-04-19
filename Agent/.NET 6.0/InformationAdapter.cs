
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Technologai
{
    public class InformationAdapter 
    {   
        private TechnologaiAgent _agent;
        private IAbility _ability;
        private Information _information;        

        // Agent
        public string AgentId => _agent.Identity.Id;
        public string WorkerId => _information.WorkerId;

        // Information
        public string ContextId => _information.Id;        
        public string CreatorId => _information.CreatorId;        
        public InformationState State => _information.State;
        public string? Input => _information.Input;
        public string? Output => _information.Output;

        // Ability
        public string AbilityId => _ability.Id;
        public string? Description => _ability.Description;
        public string? SampleJsonIn => _ability.SampleJsonIn;
        public string? SampleJsonOut => _ability.SampleJsonOut;
        public string? Prompt => _ability.Prompt;
        public string? AbilityOwnerId => _ability.MemberId;


        public InformationAdapter(TechnologaiAgent agent, IAbility ability, Information information)
        {
            _ability = ability;
            _agent = agent;
            _information = information;            
        }
 
        public InformationAdapter(TechnologaiAgent agent, Information information)
        {
            _agent = agent;
            _ability = _agent.Abilities[information.AbilityId];
            _information = information;
        }

        public static InformationAdapter Create(TechnologaiAgent _agent, string abilityId, string? input = null)
        {
            return Create(_agent, _agent.Abilities[abilityId], input);
        }

        public static InformationAdapter Create(TechnologaiAgent agent, IAbility ability, string? input = null)
        {
            var information = Information.Create(agent.Identity.Id, ability.Id, input);
            var adapter = new InformationAdapter(agent, ability, information);
            information.WorkerId = ability.MemberId ?? agent.Identity.Id;
            agent.SendStatusMessage($"{information.Id} Create> {ability.Id} | {information.Input}");
            return adapter;
        }

        protected internal async Task<Assessment> Assess()
        {
            _agent.SendStatusMessage($"{ContextId} Assess> {AbilityId} | {Input} | {Output}");
            return await _ability.Assess(this, new Assessment(_agent.Context.GetForward(ContextId), _agent.Context.GetReverse(ContextId)));
        }

        protected internal async Task<List<InformationAdapter>> Spawn(Assessment assessment)
        {
            _agent.SendStatusMessage($"{ContextId} Spawn> {AbilityId} | {Input} | {Output}");
            return ToList(await _ability.Spawn(this, assessment));
        }

        public Information Spawn(string abilityId, string? input)
        {
            var ability = _agent.Abilities[abilityId];
            var information = Information.Create(AgentId, ability.Id, input);
            _agent.Context.Spawn(information.Id, _information.Id);
            _agent.SendStatusMessage($"{information.Id} Spawn> {abilityId} | {information.Input}");
            return new InformationAdapter(_agent, ability, information);
        }


        protected internal async Task<string> Execute(Assessment assessment)
        {
            _agent.SendStatusMessage($"{ContextId} Execute> {AbilityId} | {Input} | {Output}");
            return await _ability.Execute(this, assessment);
        }

        private List<InformationAdapter> ToList(List<Information> information)
        {
            List<InformationAdapter> result = new List<InformationAdapter>();

            foreach (Information item in information)
            {
                result.Add(new InformationAdapter(_agent, item));
            }

            return result;
        }

        internal void OpenDrafts()
        {
            if (_information.State == InformationState.DRAFT)
            {
                _information.State = InformationState.OPEN;
            }
        }

        /*
        public InformationAdapter Close(string? output = null)
        {
            _information.Output = output ?? Output;
            _information.State = InformationState.CLOSED;
            _information.WorkerId = CreatorId;
            _agent.SendStatusMessage($"{ContextId} Close> {AbilityId} | {Input} | {Output}");
            return this;
        }
        */

        public async Task<InformationAdapter> Publish()
        {
            await _agent.Publish(this);
            return this;
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
