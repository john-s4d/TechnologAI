
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace Technologai
{
    public class InformationHandler : Information
    {
        internal ContextProvider Context => _agent.Context;
        private TechnologaiAgent _agent;
        //private Ability _ability;

        //internal Ability? Ability { get; set; }

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

        internal static InformationHandler CreateInformation(TechnologaiAgent _agent, Ability ability, string? input = null)
        {
            var information = new InformationHandler(_agent, input); ;
            information.AbilityName = ability.Name;
            information.SampleJsonIn = ability.SampleJsonIn;
            information.SampleJsonOut = ability.SampleJsonOut;
            information.OwnerId = ability.MemberId ?? throw new ArgumentNullException(nameof(ability.MemberId));
            _agent.SendStatusMessage($"CreateInformation > {information.ContextId} | {information.Input}");
            return information;
        }
        
        internal InformationHandler CreateInformation(TechnologaiAgent _agent, string ability, string? input = null)
        {
            return InformationHandler.CreateInformation(_agent, _agent.Abilities[ability], input);
        }

        public InformationHandler Archive()
        {
            _agent.SendStatusMessage($"Archive> {ContextId} | {Input} | {Output}");
            // Context.Archive(this);
            return this;
        }

        public InformationHandler Defer()
        {
            _agent.SendStatusMessage($"Defer> {ContextId} | {Input} | {Output}");
            //_workingQueue.Enqueue(information.ContextId);
            return this;
        }

        /*
        public InformationHandler Spawn(string? input = null)
        {
            _agent.SendStatusMessage($"Spawn> {ContextId} | {Input}");
            var new_information = CreateInformation(input);
            Context.Link(new_information, this);
            return new_information;
        }
        */
        public InformationHandler Spawn(string ability, string? input = null)
        {   
            _agent.SendStatusMessage($"Spawn> {ContextId} | {ability} | {Input}");

            var new_information = CreateInformation(_agent, ability, input);           
            Context.Link(new_information, this);
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
