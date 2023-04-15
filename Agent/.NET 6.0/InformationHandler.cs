using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Technologai
{
    public class InformationAdapter : Information
    {
        internal ContextProvider Context => _agent.Context;
        private TechnologaiAgent _agent;

        private InformationAdapter(TechnologaiAgent agent, string? input = null)
            : base(agent.Identity.Id, input)
        {
            _agent = agent;
        }

        internal InformationAdapter(Information information, TechnologaiAgent agent)
            : base(
                  information.ContextId,
                  information.CreatorId,
                  information.State,
                  information.OwnerId,
                  information.Input,
                  information.SampleJsonIn,
                  information.Output,
                  information.SampleJsonOut,
                  information.AbilityName
                  )
        {
            _agent = agent;
        }

        internal static InformationAdapter Create(TechnologaiAgent _agent, Ability ability, string? input = null)
        {
            var information = new InformationAdapter(_agent, input); ;
            information.AbilityName = ability.Name ?? throw new ArgumentNullException(nameof(ability.Name));
            information.SampleJsonIn = ability.SampleJsonIn;
            information.SampleJsonOut = ability.SampleJsonOut;
            information.OwnerId = ability.MemberId ?? _agent.Identity.Id;
            _agent.SendStatusMessage($"{information.ContextId} Create> {ability.Name} | {information.Input}");
            return information;
        }

        protected internal async Task<InformationAdapter> Execute()
        {
            _agent.SendStatusMessage($"{ContextId} Execute> {AbilityName} | {Input}");

            if (!string.IsNullOrEmpty(AbilityName) && _agent.Abilities.ContainsKey(AbilityName))
            {
                await _agent.Execute(_agent.Abilities[AbilityName], this);
            }
            else
            {
                // Don't know how to handle it..
                Assign(CreatorId);
            }
            return this;
        }

        protected internal async Task<InformationAdapter> Assess()
        {
            _agent.SendStatusMessage($"{ContextId} Assess> {AbilityName} | {Input} | {Output}");
            await _agent.Assess(this);
            return this;
        }

        protected internal async Task<InformationAdapter> Review()
        {
            _agent.SendStatusMessage($"{ContextId} Review> {AbilityName} | {Input} | {Output}");
            await _agent.Review(this);
            return this;
        }

        public InformationAdapter Archive()
        {
            _agent.SendStatusMessage($"{ContextId} Archive> {AbilityName} | {Input} | {Output}");
            // TODO: 
            return this;
        }

        public InformationAdapter Defer()
        {
            _agent.SendStatusMessage($"{ContextId} Defer> {AbilityName} | {Input} | {Output}");
            return this;
        }

        public InformationAdapter Spawn(string abilityName, string? input = null)
        {
            var new_information = Create(_agent, _agent.Abilities[abilityName], input);
            Context.Link(new_information, this);
            _agent.SendStatusMessage($"{ContextId} Spawn> {new_information.ContextId} | {abilityName} | {input}");
            return new_information;
        }

        public InformationAdapter Close(string? output = null)
        {
            Output = output;
            State = InformationState.CLOSED;
            OwnerId = CreatorId;
            _agent.SendStatusMessage($"{ContextId} Close> {AbilityName} | {Input} | {Output}");
            return this;
        }

        public InformationAdapter Assign(string ownerId)
        {
            OwnerId = ownerId;
            _agent.SendStatusMessage($"{ContextId} Assign> {AbilityName} | {Input} | {Output}");
            return this;
        }

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
    }
}
