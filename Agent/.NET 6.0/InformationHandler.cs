using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technologai
{
    public class InformationHandler : Information
    {
        internal ContextProvider Context => _agent.Context;
        private TechnologaiAgent _agent;

        protected InformationHandler(TechnologaiAgent agent)
            : base()
        {
            _agent = agent;
        }

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
                  information.SchemaIn,
                  information.Output,
                  information.SchemaOut,
                  information.AbilityName
                  )
        {   
            _agent = agent;
        }

        internal static InformationHandler CreateInformation(TechnologaiAgent _agent, string? input = null)
        {
            var information = new InformationHandler(_agent, input);
            _agent.SendStatusMessage($"CreateInformation > {information.ContextId} | {information.Input}");
            return information;
        }

        internal InformationHandler CreateInformation(string? input = null)
        {
            return InformationHandler.CreateInformation(_agent, input);
        }

        public InformationHandler Compile()
        {
            _agent.SendStatusMessage($"Compile> {ContextId} | {Input} | {Output}");
            // TODO: Compile & dispatch parent events
            return this;
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

        public InformationHandler Spawn(string? input = null)
        {
            _agent.SendStatusMessage($"Spawn> {ContextId} | {Input}");
            var new_information = CreateInformation(input);
            Context.Link(new_information, this);
            return new_information;
        }

        public InformationHandler Spawn(Ability ability)
        {
            // TODO: Merge Input JSON
            _agent.SendStatusMessage($"Spawn> {ContextId} | {AbilityName} | {Input}");
            var new_information = CreateInformation(Input);
            new_information.AbilityName = ability.Name;
            new_information.OwnerId = ability.MemberId ?? _agent.Identity.Id;
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

        public async Task<InformationHandler> Publish()
        {
            await _agent.Publish(this);
            return this;
        }
    }
}
