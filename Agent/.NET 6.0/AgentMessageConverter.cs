using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Technologai
{
    internal class AgentMessageConverter : JsonConverter<AgentMessage>
    {
        public override AgentMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            AgentMessageType? type = null;
            object? data = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName?.ToLower() == "type")
                    {
                        type = (AgentMessageType)Enum.Parse(typeof(AgentMessageType), reader.GetString() ?? throw new InvalidDataException(nameof(AgentMessageType)));
                    }
                    else if (propertyName?.ToLower() == "data")
                    {
                        if (type == AgentMessageType.INFORMATION)
                        {
                            data = JsonSerializer.Deserialize<Information>(ref reader, options);
                        }
                        else if (type == AgentMessageType.PROCESS)
                        {
                            data = JsonSerializer.Deserialize<Process>(ref reader, options);
                        }
                        else
                        {
                            throw new JsonException("Unknown type: " + type);
                        }
                    }
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new AgentMessage { Type = type, Data = data };
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, AgentMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("type", value.Type.ToString());

            writer.WritePropertyName("data");

            switch (value.Type)
            {
                case AgentMessageType.INFORMATION:
                    JsonSerializer.Serialize(writer, value.Data as Information, options);
                    break;
                case AgentMessageType.PROCESS:
                    JsonSerializer.Serialize(writer, value.Data as IProcess, options);
                    break;
                default:
                    throw new JsonException("Unknown type: " + value.Type);
            }

            writer.WriteEndObject();
        }
    }
}
