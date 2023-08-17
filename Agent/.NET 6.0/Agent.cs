using MQTTnet.Client;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Technologai
{
    public class Agent
    {
        public event EventHandler<string>? StatusMessage;
        public delegate void PublishCallback(InformationAdapter information);

        const string DISPLAY_LOG_MESSAGE = "core_display_log_message";

        public string? Name => Identity?.Name;
        public Identity Identity { get; private set; }
        public Catalog Catalog { get; private set; }
        public Context Context { get; private set; }

        private Dictionary<string, PublishCallback> _publishCallbacks = new();
        private Dictionary<string, DateTime> _knownAgents = new();        
        private MqttClient _mqtt;
        private Timer? _killTimer;

        public Agent(string authUri, string clientId, string clientSecret, string memberId)
        {
            Identity = new Identity(authUri, clientId, clientSecret, memberId);

            Catalog = new Catalog(Identity);
            Context = new Context(Identity);

            _mqtt = new MqttClient(Identity, _mqtt_MessageReceived);            
        }

        // ** TRANSPORT **

        private async void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            var brokerMessage = BrokerMessage.FromMqttArgs(args);

            if (brokerMessage.MessageType == AgentMessageType.PULSE)
            {
                PulseMessage? pulse = brokerMessage.MessageData as PulseMessage;

                if (pulse != null && pulse.MemberId != Identity.Id)
                {
                    await Receive(pulse);
                }
            }

            else if (brokerMessage.MessageType == AgentMessageType.TEMPLATE)
            {
                Template? template = brokerMessage.MessageData as Template;

                if (template != null && template.MemberId != Identity.Id)
                {
                    await SendStatusMessage($"{template.MemberId} {template.Id} template receive");
                    Catalog.Add(template);
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

        private async Task SendPulse(string memberId = "0")
        {
            await SendStatusMessage($"{memberId} pulse send");

            var brokerMessage = new BrokerMessage(Identity)
            {
                MessageType = AgentMessageType.PULSE,
                MessageData = new PulseMessage() { MemberId = Identity.Id },
                MemberId = memberId
            };

            string messageJson = brokerMessage.ConvertMessageDataToString();

            await _mqtt.PublishAsync(brokerMessage.Topic, messageJson, brokerMessage.MessageType);
        }

        private async Task Receive(PulseMessage pulse)
        {
            await SendStatusMessage($"{pulse.MemberId} pulse receive");

            if (pulse != null && !string.IsNullOrEmpty(pulse.MemberId))
            {
                if (!_knownAgents.ContainsKey(pulse.MemberId))
                {
                    await SendPulse(pulse.MemberId);
                }

                _knownAgents[pulse.MemberId] = DateTime.UtcNow;

                foreach (var template in Catalog.Values)
                {
                    if (template.MemberId == Identity.Id)
                    {
                        await Send(template, pulse.MemberId);
                    }
                }
            }
        }

        public async Task Send(ITemplate template, string memberId)
        {
            await SendStatusMessage($"{memberId} {template.Id} template send");

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

                // Invoke the callback.
                if (_publishCallbacks.ContainsKey(information.Id))
                {
                    _publishCallbacks[information.Id]?.Invoke(information);
                    _publishCallbacks.Remove(information.Id);
                }

                // Activate the parent information
                var parentInformation = Context.GetCreator(information.Id);

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

        public async Task<InformationAdapter> CreateInformation(string templateId, Data? input = null)
        {
            //_active[information.ContextId] = information;  
            return await InformationAdapter.Create(this, (Template)Catalog[templateId], input);
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

            // TODO: This can wait indefinitly if the information is never closed or template doesn't exist. Add timeout / decay.

            while (!callbackComplete)
            {
                await Task.Delay(10);
            }

            return result;
        }

        public async Task Publish(Information information, PublishCallback? publishCallback = null)
        {
            if (information.TemplateId != DISPLAY_LOG_MESSAGE)
            {
                _ = SendStatusMessage($"{information.Id} Publish> {information.TemplateId} | {information.InformationState} | {information.Input} | {information.Output}");
            }

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

        public async Task SendStatusMessage(string message)
        {

            if (_mqtt.IsConnected && Catalog.ContainsKey(DISPLAY_LOG_MESSAGE) && Catalog[DISPLAY_LOG_MESSAGE].MemberId != null && Catalog[DISPLAY_LOG_MESSAGE].MemberId != Identity.Id)
            {
                var information = await CreateInformation(DISPLAY_LOG_MESSAGE, $"{Name?.PadRight(21)} | {message}");
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

            //this.Name = Identity.Name;

            await SendStatusMessage($"Authenticated");

            await _mqtt.ConnectAsync();
            await SendStatusMessage($"Connected");

            await _mqtt.SubscribeAsync(Identity.SubscribeMemberMask);
            await SendStatusMessage($"Subscribed {Identity.SubscribeMemberMask}");

            await _mqtt.SubscribeAsync(Identity.SubscribeAgencyMask);
            await SendStatusMessage($"Subscribed {Identity.SubscribeAgencyMask}");

            await SendPulse();

            await Task.Delay(5000); // Wait here for a bit to sync up Templates
        }

        public async Task Stop()
        {
            await _mqtt.DisconnectAsync();
        }

        public async void Kill(double delayMs = 0)
        {
            if (delayMs == 0)
            {
                await KillTimerCallback();
            }
            else
            {
                _killTimer = new Timer(delayMs);
                _killTimer.Elapsed += async (sender, e) => await KillTimerCallback();
                _killTimer.Start();
            }
        }

        private async Task KillTimerCallback()
        {
            await _mqtt.DisconnectAsync();
            Environment.Exit(0);
        }
    }
}