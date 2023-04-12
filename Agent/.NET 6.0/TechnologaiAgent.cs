using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Client;
using System.ComponentModel.Design;
using System.Net.Http;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public string? Name { get; private set; }

        public event EventHandler<string>? StatusMessage;

        private MqttClient _mqtt;
        private ContextProvider _contexts;
        private ActionCatalog _actions;
        //private PromptCatalog _prompts;

        public MemberIdentity Identity { get; } // TODO: Connect as an Agent, without memberId

        public string IdentityId { get { return string.IsNullOrEmpty(Identity.Id) ? throw new NullReferenceException(nameof(Identity.Id)) : Identity.Id; } }

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            Identity = new MemberIdentity(memberId, new AgentIdentity(authorityName, clientId, clientSecret));

            _contexts = new ContextProvider(Identity);
            _mqtt = new MqttClient(Identity);
            _actions = new ActionCatalog();

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var information = BrokerMessage.FromMqttArgs(args).Information;

            if (information != null)
            {
                _ = Receive(information);
            }
        }

        private async Task Receive(Information information)
        {
            if (await BeforeHandle(information))
            {
                await Handle(information);
            }
            await AfterHandle(information);
        }

        private async Task<bool> BeforeHandle(Information information)
        {
            if (information.State == InformationState.DRAFT)
            {
                // Problem. Agents should not receive drafts from other agents.
                return false;
            }

            if (information.OwnerId == null)
            {
                // There is no owner
                if (information.State == InformationState.OPEN)
                {
                    // MEMBER-CREATOR: Self-Assign.
                    // MEMBER-WORKER: Handle. Determine if I can own this.
                    // AGENCY: Handle. Find an owner.
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER-CREATOR: Self-Assign.
                    // MEMBER-WORKER: Redirect. TODO: Send to Agency.
                    // AGENCY: Handle. Post-processing & analysis. Note: multiple instances.
                }
            }
            else if (information.OwnerId == IdentityId)
            {
                // I am the owner
                if (information.State == InformationState.OPEN)
                {
                    // MEMBER-CREATOR: Handle. This was returned to me and is incomplete.
                    // MEMBER-WORKER: Handle. TODO: Add to my work queue.
                    // AGENCY: Problem. Agencies should not own open work.
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER-CREATOR: Handle. This was returned to me and is complete.
                    // MEMBER-WORKER: Redirect. TODO: Send to creator.
                    // AGENCY: Handle. Post-processing & analysis.
                }
            }
            else
            {
                // Someone else is the owner
                if (information.State == InformationState.OPEN)
                {
                    // MEMBER: Redirect. TODO: Redirect to Owner
                    // AGENCY: Redirect. TODO: Redirect to Owner
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER: Redirect. TODO: Redirect to Owner
                    // AGENCY: Redirect. TODO: Redirect to Owner 
                }
            }

            return true;
        }

        public virtual Task Handle(Information information)
        {
            return Task.CompletedTask;
        }

        private async Task AfterHandle(Information information)
        {

            if (information.State == InformationState.DRAFT)
            {
                if (information.OwnerId == IdentityId)
                {
                    // TODO: Save to my drafts. Will this ever happen?
                }
                return;                
            }

            if (information.OwnerId == null)
            {
                // There is no owner
                 if (information.State == InformationState.OPEN)
                {
                    // MEMBER: Publish. To Agency for assignment.
                    // AGENCY: Problem. Couldn't find an owner. // Candidate for system capability improvements.
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER: Publish. To Agency for archiving.
                    // AGENCY: Problem. This should have already been handled.
                }
            }
            else if (information.OwnerId == IdentityId)
            {
                // I am the owner
                if (information.State == InformationState.OPEN)
                {
                    // MEMBER: Add to my work queue. Don't publish.
                    // AGENCY: Problem. Agencies should not own open information.
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER: Update upstream contexts
                    // AGENCY: Post-processing & analysis 
                }
            }
            else
            {
                // Someone else is the owner
                if (information.State == InformationState.OPEN)
                {
                    // MEMBER: Publish. 
                    // AGENCY: Publish. 
                }
                else if (information.State == InformationState.CLOSED)
                {
                    // MEMBER: Publish. 
                    // AGENCY: Publish. 
                }
            }
        }

        // Information Handling
        public async Task Spawn(Information information, string input)
        {
            Information new_information = _contexts.Spawn(information, input);
            SendStatusMessage($"{Name} Spawn> {information.ContextId} | {information.Input} ");
            await Publish(new_information);
        }

        public async Task CreateInformation(string input)
        {
            Information new_information = _contexts.CreateInformation(input);
            SendStatusMessage($"{Name} Create> {new_information.ContextId} | {new_information.Input} ");
            await Publish(new_information);
        }

        public void Complete(Information information, string output)
        {
            information.State = InformationState.COMPLETE;
            information.Output = output;
        }

        public void Assign(Information information, string ownerId)
        {
            information.OwnerId = ownerId;
        }

        private async Task<bool> Publish(Information information, string? memberId = null)
        {
            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = memberId
            };

            return await Publish(message);
        }

        private async Task<bool> Publish(BrokerMessage message)
        {
            if (message.Information == null)
            {
                return false;
            }

            Identity? publishIdentity = null;

            if ((Identity.Agency != null) && (Identity.AssignedRole?.Equals("agency") ?? false))
            {
                publishIdentity = Identity.Agency;
            }
            if (Identity.AssignedRole?.Equals("member") ?? false)
            {
                publishIdentity = Identity;
            }
            if (Identity.AssignedRole?.Contains("agent") ?? false)
            {
                publishIdentity = Identity.Agent;
            }

            if (publishIdentity != null)
            {
                string? topic = publishIdentity?.GetMaskedTopic(message.Topic);

                await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

                SendStatusMessage($"Published: {message.Information.ContextId} {topic}");

                return true;
            }

            return false;
        }

        private void SendStatusMessage(string message)
        {
            StatusMessage?.Invoke(this, message);
        }

        public async Task Start()
        {
            try
            {
                // TODO: Fix in AI-17
                SendStatusMessage($"Warming up...");
                await new HttpClient().GetAsync($"https://{Identity.Authority.Host}/.well-known/jwks.json");
                await new HttpClient().GetAsync($"https://{Identity.Authority.Host}/.well-known/openid-configuration");

                await Identity.Authenticate();

                this.Name = Identity.Name;

                SendStatusMessage($"Authenticated");

                await _mqtt.ConnectAsync();
                SendStatusMessage($"Connected");

                // Subscribe to Member Topics
                if (Identity.AssignedRole?.Equals(Identity.RoleName) ?? false)
                {
                    await _mqtt.SubscribeAsync(Identity.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.RoleName}:{Identity.SubscribeMask}");
                }

                // Subscribe to Agency Topics
                if (Identity.Agency != null && (Identity.AssignedRole?.Equals(Identity.Agency.RoleName) ?? false))
                {
                    await _mqtt.SubscribeAsync(Identity.Agency.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agency.RoleName}:{Identity.Agency.SubscribeMask}");
                }

                // Subscribe to Agent Topics
                if (Identity.Agent != null && (Identity.AssignedRole?.Equals(Identity.Agent.RoleName) ?? false))
                {
                    await _mqtt.SubscribeAsync(Identity.Agent.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agent.RoleName}:{Identity.Agent.SubscribeMask}");
                }

            }
            catch (Exception ex)
            {
                SendStatusMessage(ex.ToString());
            }
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}