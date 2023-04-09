using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using System.Net.Http;

namespace Technologai
{
    public class TechnologaiAgent
    {
        public string? Name { get; private set; }

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<string>? StatusMessage;

        private MqttClient _mqtt;

        private readonly MemberIdentity _identity;
        // TODO: Connect as an Agent, without memberId

        public TechnologaiAgent(string authorityName, string clientId, string clientSecret, string memberId)
        {
            _identity = new MemberIdentity(memberId, new AgentIdentity(authorityName, clientId, clientSecret));

            _mqtt = new MqttClient(_identity);

            _mqtt.MessageReceived += _mqtt_MessageReceived;
        }

        private void _mqtt_MessageReceived(object? sender, MqttApplicationMessageReceivedEventArgs args)
        {
            // TODO: Get details of the message. Convert into Standardized Message Type
            ReceiveMessage(args.ApplicationMessage.ConvertPayloadToString());
        }

        public async Task PublishMessageToAgency(string message, string context = "0")
        {
            if (_identity.AssignedRoles.Contains(_identity.RoleName))
            {
                if (context.Contains("+") || context.Contains("/"))
                {
                    throw new ArgumentException(nameof(context));
                }

                string topic = _identity.PublishMask.Replace("+", context);
                await _mqtt.PublishAsync(topic, message);
                //SendStatusMessage($"Published: {_identity.RoleName}:{_identity.SubscribeMask}");
            }
            else
            {
                // Only Members 
                // Let client know it's not allowed
            }
        }

        public async Task PublishMessageToMember(string message, string? memberId, string context = "0")
        {
            if (_identity.Agency != null && _identity.AssignedRoles.Contains(_identity.Agency.RoleName))
            {
                try
                {
                    if (memberId != null)
                    {
                        Base64UrlEncoder.DecodeBytes(memberId);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(nameof(memberId));
                }

                if (context.Contains("+") || context.Contains("/"))
                {
                    throw new ArgumentException(nameof(context));
                }
                var topicParts = _identity.Agency.PublishMask.Split('/');
                topicParts[1] = topicParts[1].Replace("+", memberId ?? _identity.Id);
                topicParts[2] = topicParts[2].Replace("+", context);
                await _mqtt.PublishAsync(string.Join('/', topicParts), message);
                //SendStatusMessage($"Published: {_identity.Agency.RoleName}:{_identity.Agency.SubscribeMask}");
            }
            else
            {
                // Let client know it's not allowed
            }
        }

        public async Task PublishMessageToAgent(string message, string subagent = "0")
        {
            if (_identity.AssignedRoles.Contains(_identity.Agent.RoleName))
            {
                if (subagent.Contains("+") || subagent.Contains("/"))
                {
                    throw new ArgumentException(nameof(subagent));
                }

                string topic = _identity.Agent.PublishMask.Replace("+", subagent);
                await _mqtt.PublishAsync(topic, message);
                //SendStatusMessage($"Published: {_identity.Agent.RoleName}:{_identity.Agent.SubscribeMask}");
            }
            else
            {   
                // Let client know it's not allowed
            }
        }


        private void ReceiveMessage(string message)
        {
            MessageReceived?.Invoke(this, message);
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
                await new HttpClient().GetAsync($"https://{_identity.Authority.Host}/.well-known/jwks.json");
                await new HttpClient().GetAsync($"https://{_identity.Authority.Host}/.well-known/openid-configuration");

                await _identity.Authenticate();

                this.Name = _identity.Name;

                SendStatusMessage($"Authenticated");

                await _mqtt.ConnectAsync();
                SendStatusMessage($"Connected");

                // Subscribe to Member Topics
                if (_identity.AssignedRoles.Contains(_identity.RoleName))
                {
                    await _mqtt.SubscribeAsync(_identity.SubscribeMask);
                    SendStatusMessage($"Subscribed: {_identity.RoleName}:{_identity.SubscribeMask}");
                }

                // Subscribe to Agency Topics
                if (_identity.Agency != null && _identity.AssignedRoles.Contains(_identity.Agency.RoleName))
                {
                    await _mqtt.SubscribeAsync(_identity.Agency.SubscribeMask);
                    SendStatusMessage($"Subscribed: {_identity.Agency.RoleName}:{_identity.Agency.SubscribeMask}");
                }

                // Subscribe to Agent Topics
                if (_identity.Agent != null && _identity.AssignedRoles.Contains(_identity.Agent.RoleName))
                {
                    await _mqtt.SubscribeAsync(_identity.Agent.SubscribeMask);
                    SendStatusMessage($"Subscribed: {_identity.Agent.RoleName}:{_identity.Agent.SubscribeMask}");
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