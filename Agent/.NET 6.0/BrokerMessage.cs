using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System.Text.Json;

namespace Technologai
{
    public enum AgentMessageType
    {
        HELLO,
        NEURON,
        INFORMATION,
        CONTEXT
    }

    public class BrokerMessage
    {
        internal const string MESSAGE_TYPE = "messagetype";
        private const string TOPIC_DELIMITER = "/";

        public string? AgencyId { get; set; }
        public string? MemberId { get; set; }
        public string Topic { get { return $"{AgencyId ?? "-"}/{MemberId ?? "-"}"; } }
        public AgentMessageType MessageType { get; set; }
        public object? MessageData { get; set; }

        private BrokerMessage() { }

        internal BrokerMessage(Identity identity)
        {
            AgencyId = identity.AgencyId;
        }

        internal static BrokerMessage FromMqttArgs(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicParts = args.ApplicationMessage.Topic.Split(TOPIC_DELIMITER);

            var brokerMessage = new BrokerMessage()
            {
                AgencyId = topicParts[0],
                MemberId = topicParts[1],
            };

            var payload = args.ApplicationMessage.ConvertPayloadToString();

            foreach (MqttUserProperty property in args.ApplicationMessage.UserProperties)
            {
                if (property.Name == MESSAGE_TYPE)
                {   
                    switch (property.Value)
                    {
                        case "HELLO":
                            brokerMessage.MessageType = AgentMessageType.HELLO;
                            brokerMessage.MessageData = JsonSerializer.Deserialize<HelloMessage>(payload);
                            break;
                        case "PROCESS":
                            brokerMessage.MessageType = AgentMessageType.NEURON;
                            brokerMessage.MessageData = JsonSerializer.Deserialize<Neuron>(payload);
                            break;
                        case "INFORMATION":
                            brokerMessage.MessageType = AgentMessageType.INFORMATION;
                            brokerMessage.MessageData = JsonSerializer.Deserialize<Information>(payload);
                            break;
                    }
                    break;
                }
            }
            return brokerMessage;
        }

        internal string ConvertMessageDataToString()
        {
            switch (MessageType)
            {
                case AgentMessageType.HELLO:
                    return JsonSerializer.Serialize(MessageData as HelloMessage);
                case AgentMessageType.NEURON:
                    return JsonSerializer.Serialize(MessageData as Neuron);
                case AgentMessageType.INFORMATION:
                    return JsonSerializer.Serialize(MessageData as Information);
                default:
                    throw new InvalidDataException($"Unknown message type: {MessageType}");
            }
        }
    }
}
