using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Client;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;

namespace Technologai
{
    public enum Role
    {
        Agency,
        Member,
        Agent
    }

    public class TechnologaiAgent
    {
        public string? Name { get; private set; }

        public event EventHandler<string>? StatusMessage;

        private MqttClient _mqtt;
        private ContextProvider _contexts;
        private ActionCatalog _actions;

        private Dictionary<string, Information> _information = new Dictionary<string, Information>();

        private Queue<string> _workingQueue = new Queue<string>();
        private Queue<string> _archiveQueue = new Queue<string>();        

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
                await Publish(information);
            }
        }

        private async Task<bool> BeforeHandle(Information information)
        {
            var isNullOwned = information.OwnerId == null;
            var isSelfOwned = information.OwnerId == IdentityId;
            var isElseOwned = information.OwnerId != IdentityId && information.OwnerId != null;
            var isDraft = information.State == InformationState.DRAFT;
            var isOpen = information.State == InformationState.OPEN;
            var isClosed = information.State == InformationState.CLOSED;
            var isAgency = Identity.AssignedRole == TechnologaiRole.agency;
            var isMember = Identity.AssignedRole == TechnologaiRole.member;
            var isCreator = information.CreatorId == IdentityId;

            if (isDraft)
            {
                return false;
            }

            if (isElseOwned)
            {
                await SendToBroker(information, information.OwnerId);
                return false;
            }

            if (_information.ContainsKey(information.ContextId)) {
                _information.Add(information.ContextId, information);
            }

            if (isMember)
            {
                if (isCreator && isOpen)
                {
                    Close(information); // Incomplete
                    Defer(information);
                    return false;
                }

                if (isCreator && isClosed)
                {
                    Compile(information);
                    return false;
                }

                if (isClosed)
                {
                    await SendToBroker(information, information.CreatorId);
                    return false;
                }
            }

            else if (isAgency && isNullOwned && isClosed)
            {
                Archive(information);
                return false;
            }

            return true;
        }

        public virtual Task Handle(Information information)
        {
            return Task.CompletedTask;
        }

        public async Task Publish(Information information)
        {
            var isNullOwned = information.OwnerId == null;
            var isSelfOwned = information.OwnerId == IdentityId;
            var isElseOwned = information.OwnerId != IdentityId && information.OwnerId != null;
            var isDraft = information.State == InformationState.DRAFT;
            var isOpen = information.State == InformationState.OPEN;
            var isClosed = information.State == InformationState.CLOSED;
            var isAgency = Identity.AssignedRole == TechnologaiRole.agency;
            var isMember = Identity.AssignedRole == TechnologaiRole.member;
            var isCreator = information.CreatorId == IdentityId;

            if (isDraft)
            {
                information.State = InformationState.OPEN;
            }

            if (_information.ContainsKey(information.ContextId)) {
                _information.Add(information.ContextId, information);
            }

            if (isElseOwned)
            {
                await SendToBroker(information, information.OwnerId);
                return;
            }

            if (isSelfOwned && isOpen)
            {
                Defer(information);
                return;
            }

            if (isMember)
            {
                if (isNullOwned && (isCreator || isOpen))
                {
                    await SendToBroker(information);
                    return;
                }
                await SendToBroker(information, information.CreatorId);
                return;
            }

            if (isAgency)
            {
                Archive(information);
                return;
            }

            throw new NotImplementedException("information was not routed");
            
            // TODO: What if I am an agent.

        }

        // Information Handling

        private void Compile(Information information)
        {
            SendStatusMessage($"Compile > {information.ContextId} | {information.Input} | {information.Output}");
            // TODO: Compile & dispatch parent events
        }

        private void Archive(Information information)
        {
            SendStatusMessage($"Archive > {information.ContextId} | {information.Input} | {information.Output}");
            _archiveQueue.Enqueue(information.ContextId);
        }

        private void Defer(Information information)
        {
            SendStatusMessage($"Defer > {information.ContextId} | {information.Input} | {information.Output}");
            _workingQueue.Enqueue(information.ContextId);
        }
        
        public Information Spawn(Information information, string? input = null)
        {
            var new_information = _contexts.Spawn(information, input);
            SendStatusMessage($"Spawn > {information.ContextId} | {information.Input} | {information.Output}");
            return new_information;
        }

        public Information CreateInformation(string? input = null)
        {
            var information = _contexts.CreateInformation(input);
            SendStatusMessage($"CreateInformation > {information.ContextId} | {information.Input} | {information.Output}");
            return information;
        }

        public void Close(Information information, string? output = null)
        {   
            information.Output = output;
            information.State = InformationState.CLOSED;
            information.OwnerId = information.CreatorId;
            SendStatusMessage($"Close > {information.ContextId} | {information.Input} | {information.Output}");
        }

        public void Assign(Information information, string ownerId)
        {
            information.OwnerId = ownerId;
            SendStatusMessage($"Assign > {information.ContextId} | {information.Input} | {information.Output}");
        }

        private async Task<bool> SendToBroker(Information information, string? memberId = null)
        {
            var message = new BrokerMessage(Identity)
            {
                Information = information,
                MemberId = memberId
            };

            return await SendToBroker(message);
        }

        private async Task<bool> SendToBroker(BrokerMessage message)
        {
            if (message.Information == null)
            {
                return false;
            }

            Identity? publishIdentity = null;

            if (Identity.AssignedRole == TechnologaiRole.agency)
            {
                publishIdentity = Identity.Agency;
            }
            if (Identity.AssignedRole == TechnologaiRole.member)
            {
                publishIdentity = Identity;
            }
            if (Identity.AssignedRole == TechnologaiRole.agent)
            {
                publishIdentity = Identity.Agent;
            }

            if (publishIdentity != null)
            {
                string? topic = publishIdentity?.GetMaskedTopic(message.Topic);

                await _mqtt.PublishAsync(topic ?? string.Empty, message.Information.ToJson());

                //SendStatusMessage($"Published: {message.Information.ContextId} {topic}");

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
                if (Identity.AssignedRole == TechnologaiRole.member)
                {
                    await _mqtt.SubscribeAsync(Identity.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.RoleName}:{Identity.SubscribeMask}");
                }

                // Subscribe to Agency Topics
                if (Identity.AssignedRole == TechnologaiRole.agency)
                {
                    await _mqtt.SubscribeAsync(Identity.Agency.SubscribeMask);
                    SendStatusMessage($"Subscribed: {Identity.Agency.RoleName}:{Identity.Agency.SubscribeMask}");
                }

                // Subscribe to Agent Topics
                if (Identity.AssignedRole == TechnologaiRole.agent)
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