using MQTTnet.Client;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Technologai
{
    public class Agent
    {
        public event EventHandler<string>? StatusMessage;
        public delegate void PublishCallback(InformationAdapter information);

        public string? Name { get; private set; }
        public Catalog Catalog { get; private set; }        
        public Identity Identity { get; }
        public bool IsConnected => _mqtt.IsConnected;
        internal Context Context { get; private set; }

        private Dictionary<string, PublishCallback> _publishCallbacks = new Dictionary<string, PublishCallback>();
        private MqttClient _mqtt;
        private List<string> _localAgents = new List<string>();

        public Agent(string authUri, string clientId, string clientSecret, string memberId)
        {
            Identity = new Identity(authUri, clientId, clientSecret, memberId);
            Catalog = new Catalog(Identity, this);
            Context = new Context(Identity);

            _mqtt = new MqttClient(Identity);
            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        // ** TRANSPORT **

        private async void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var brokerMessage = BrokerMessage.FromMqttArgs(args);

            if (brokerMessage.MessageType == AgentMessageType.INTRODUCTION)
            {
                IntroductionMessage? introduction = brokerMessage.MessageData as IntroductionMessage;

                if (introduction != null && introduction.MemberId != Identity.Id)
                {
                    await Receive(introduction);
                }
            }

            else if (brokerMessage.MessageType == AgentMessageType.TEMPLATE)
            {
                Template? template = brokerMessage.MessageData as Template;

                if (template != null && template.MemberId != Identity.Id)
                {
                    Catalog.Add(template);
                    await SendStatusMessage($"Received template: {template.Id}");
                }
            }

            else if (brokerMessage.MessageType == AgentMessageType.INFORMATION)
            {
                Information? information = brokerMessage.MessageData as Information;

                if (information != null)
                {
                    await Receive(InformationAdapter.Create(this, information));
                }
            }
        }

        private async Task SendIntroduction(string toMemberId = "0")
        {
            await SendStatusMessage($"Broadcasting introduction");

            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.INTRODUCTION,
                MessageData = new IntroductionMessage() { MemberId = Identity.Id },
                MemberId = toMemberId
            };

            string messageJson = brokerMessage.ConvertMessageDataToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, messageJson, brokerMessage.MessageType);
        }

        private async Task Receive(IntroductionMessage introduction)
        {
            await SendStatusMessage($"Received introduction from: {introduction.MemberId}");

            if (introduction != null && !string.IsNullOrEmpty(introduction.MemberId))
            {
                if (!_localAgents.Contains(introduction.MemberId))
                {
                    _localAgents.Add(introduction.MemberId);
                }

                foreach (var template in Catalog.Values)
                {
                    if (template.MemberId == Identity.Id)
                    {
                        await Send(template, introduction.MemberId);
                    }
                }
            }
        }

        public async Task Send(Template template, string memberId)
        {
            await SendStatusMessage($"Sending: {template.Id} to {memberId}");

            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.TEMPLATE,
                MessageData = template,
                MemberId = memberId
            };

            string messageJson = brokerMessage.ConvertMessageDataToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, messageJson, brokerMessage.MessageType);
        }

        private async Task Receive(InformationAdapter information)
        {
            Context.Add(information);

            // Closed and this agent is the creator
            if (information.InformationState == InformationState.CLOSED && information.CreatorId == Identity.Id)
            {
                //_active.Remove(information.ContextId);

                // Activate the calling information
                var parentInformation = Context.GetCreator(information.Id);

                // Invoke the callback.
                if (_publishCallbacks.ContainsKey(information.Id))
                {
                    _publishCallbacks[information.Id]?.Invoke(information);
                }

                if (parentInformation == null)
                {
                    // This is a root request. 
                    return;
                }

                information = InformationAdapter.Create(this, parentInformation);

                // -> Fall through to next if condition
            }

            // Open, and this agent is assigned
            if (information.InformationState == InformationState.OPEN && information.WorkerId == Identity.Id)
            {
                //_active[information.ContextId] = information;

                // TODO: Debounce

                if (await information.Assess())
                {
                    await information.Process();
                }
            }

            // Closed, and this agent is not the creator
            if (information.InformationState == InformationState.CLOSED && information.CreatorId != Identity.Id)
            {
                // TODO: Review. Add to Context.
            }
        }

        public async Task<InformationAdapter> Create(string templateId, string? input = null)
        {
            //_active[information.ContextId] = information;  
            return await InformationAdapter.Create(this, Catalog[templateId], input);
        }

        internal async Task<Data?> PublishAndWait(InformationAdapter information)
        {
            bool callbackComplete = false;

            Data? result = null;

            await Publish(information, (returnedInformation) =>
                {
                    result = returnedInformation.Output;
                    callbackComplete = true;
                }
            );

            while (!callbackComplete)
            {
                await Task.Delay(100); // TODO: Reduce delay to something more responsive
            }

            return result;
        }

        public async Task Publish(Information information, PublishCallback? publishCallback = null)
        {
            //_ = SendStatusMessage($"{information.Id} Publish> {information.templateId} | {information.InputText} | {information.OutputText}");

            if (publishCallback != null)
            {
                _publishCallbacks.Add(information.Id, publishCallback);
            }

            if (information.InformationState == InformationState.DRAFT)
            {
                information.InformationState = InformationState.OPEN;
            }

            // Short circuit
            if (Identity.Id == information.WorkerId)
            {
                new Task(async () =>
                {
                    await Receive(InformationAdapter.Create(this, information));
                }).Start();
                return;
            }

            // Long route
            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.INFORMATION,
                MessageData = information,
                MemberId = information.WorkerId
            };

            var messageJson = brokerMessage.ConvertMessageDataToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, messageJson, brokerMessage.MessageType);
        }

        internal async Task SendStatusMessage(string message)
        {
            if (_mqtt.IsConnected && Catalog.ContainsKey("display_log_message") && Catalog["display_log_message"].MemberId != null && Catalog["display_log_message"].MemberId != Identity.Id)
            {
                var information = await Create("display_log_message", message);
                await information.Publish();
            }
            else
            {
                StatusMessage?.Invoke(this, message);
            }
        }

        // Startup

        public async Task Start()
        {
            // TODO: Fix in AI-17
            await SendStatusMessage($"Warming up...");
            await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/jwks.json");
            await new HttpClient().GetAsync($"{Identity.Authority.AuthUri}/.well-known/openid-configuration");

            await Identity.Authenticate(Identity.Authority.BrokerUri);

            this.Name = Identity.Name;

            await SendStatusMessage($"Authenticated");

            await _mqtt.ConnectAsync();
            await SendStatusMessage($"Connected");

            await _mqtt.SubscribeAsync(Identity.SubscribeAgencyMask);
            await SendStatusMessage($"Agency Subscribed> {Identity.SubscribeAgencyMask}");

            await _mqtt.SubscribeAsync(Identity.SubscribeMemberMask);
            await SendStatusMessage($"Member Subscribed> {Identity.SubscribeMemberMask}");

            await SendIntroduction();
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }
    }
}