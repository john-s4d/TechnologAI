
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Technologai
{
    public class InformationAdapter : Information
    {
        private TechnologaiAgent _agent;

        private InformationAdapter(TechnologaiAgent agent, string abilityName, string? input = null)
            : base(agent.Identity.Id, abilityName, input)
        {
            _agent = agent;
        }



        internal InformationAdapter(Information information, TechnologaiAgent agent)
            : base(
                  information.Id,
                  information.CreatorId,
                  information.AbilityName,
                  information.State,
                  information.OwnerId,
                  information.Input,
                  information.SampleJsonIn,
                  information.Output,
                  information.SampleJsonOut,
                  information.Signature
                  )
        {
            _agent = agent;
        }

        internal static InformationAdapter Create(TechnologaiAgent _agent, string abilityName, string? input = null)
        {
            return Create(_agent, _agent.Abilities[abilityName], input);
        }

        internal static InformationAdapter Create(TechnologaiAgent _agent, Ability ability, string? input = null)
        {
            var information = new InformationAdapter(_agent, ability.Name, input);
            information.SampleJsonIn = ability.SampleJsonIn;
            information.SampleJsonOut = ability.SampleJsonOut;
            information.OwnerId = ability.MemberId ?? _agent.Identity.Id;
            _agent.SendStatusMessage($"{information.Id} Create> {ability.Name} | {information.Input}");
            return information;
        }

        /*
        public async Task<InformationAdapter> Execute()
        {
            _agent.SendStatusMessage($"{Id} Execute> {AbilityName} | {Input}");
            await _agent.Execute(this);
            return this;
        }*/

        protected internal async Task<bool> Assess()
        {
            _agent.SendStatusMessage($"{Id} Assess> {AbilityName} | {Input} | {Output}");
            return await _agent.Assess(this, _agent.Context.GetForward(this.Id), _agent.Context.GetReverse(this.Id));            
        }

        protected internal async Task<List<Information>> Spawn()
        {
            return await _agent.Spawn(this, _agent.Context.GetForward(this.Id), _agent.Context.GetReverse(this.Id));
        }

        protected internal async Task<Information> Execute()
        {
            return await _agent.Execute(this, _agent.Context.GetForward(this.Id), _agent.Context.GetReverse(this.Id));
        }


        public InformationAdapter Close(string? output = null)
        {
            Output = output ?? Output;
            State = InformationState.CLOSED;
            OwnerId = CreatorId;
            _agent.Close(this);
            _agent.SendStatusMessage($"{Id} Close> {AbilityName} | {Input} | {Output}");
            return this;
        }
        
        public InformationAdapter Assign(string ownerId)
        {
            OwnerId = ownerId;
            _agent.SendStatusMessage($"{Id} Assign> {AbilityName} | {Input} | {Output}");
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
