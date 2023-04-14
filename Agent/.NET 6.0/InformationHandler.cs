
using Newtonsoft.Json;

namespace Technologai
{
    public class InformationHandler : Information
    {
        internal ContextProvider Context => _agent.Context;
        private TechnologaiAgent _agent;

        private InformationHandler(TechnologaiAgent agent, string? input = null)
            : base(agent.Identity.Id, input)
        {
            _agent = agent;
        }

        internal InformationHandler(Information information, TechnologaiAgent agent)
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

        internal static InformationHandler Create(TechnologaiAgent _agent, Ability ability, string? input = null)
        {
            var information = new InformationHandler(_agent, input); ;
            information.AbilityName = ability.Name;
            information.SampleJsonIn = ability.SampleJsonIn;
            information.SampleJsonOut = ability.SampleJsonOut;
            information.OwnerId = ability.MemberId ?? _agent.Identity.Id;
            _agent.SendStatusMessage($"Create> {information.ContextId} | {ability.Name} | {information.Input}");
            return information;
        }
        
        internal InformationHandler Create(TechnologaiAgent _agent, string ability, string? input = null)
        {
            return InformationHandler.Create(_agent, _agent.Abilities[ability], input);
        }

        public InformationHandler Archive()
        {
            _agent.SendStatusMessage($"Archive> {ContextId} | {Input} | {Output}");            
            return this;
        }

        public InformationHandler Defer()
        {
            _agent.SendStatusMessage($"Defer> {ContextId} | {Input} | {Output}");            
            return this;
        }

        public InformationHandler Spawn(string ability, string? input = null)
        {   
            var new_information = Create(_agent, ability, input);           
            Context.Link(new_information, this);
            _agent.SendStatusMessage($"Spawn> {ContextId} => {new_information.ContextId} | {ability} | {input}");
            return new_information;
        }

        public InformationHandler Close(string? output = null)
        {
            Output = output;
            State = InformationState.CLOSED;
            OwnerId = CreatorId;
            _agent.SendStatusMessage($"Close> {ContextId} | {Input} | {Output}");
            return this;
        }

        public InformationHandler Assign(string ownerId)
        {
            OwnerId = ownerId;
            _agent.SendStatusMessage($"Assign> {ContextId} | {Input} | {Output}");
            return this;
        }

        public async Task<InformationHandler> Execute()
        {

            await _agent.Execute(AbilityName, this);
            return this;
        }

        public async Task<InformationHandler> Compile()
        {
            await _agent.Compile(this);
            return this;
        }

        public async Task<InformationHandler> Publish()
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
