using System.Text.Json;
using System.Text.Json.Serialization;

namespace Technologai
{
    public class DataJsonConverter : JsonConverter<Data>
    {
        public override Data Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            DataFormat? dataFormat = null;
            string? raw = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == "Format")
                    {
                        dataFormat = (DataFormat)reader.GetInt32();
                    }
                    else if (propertyName == "Raw")
                    {
                        raw = reader.GetString();
                    }
                }

                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
            }

            if (dataFormat.HasValue)
            {
                return new Data(raw, dataFormat.Value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Data value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Format", (int)value.Format);
            writer.WriteString("Raw", value.Raw);
            writer.WriteEndObject();
        }
    }

    public enum DataFormat
    {
        RAW = 0,
        STRUCTURED = 1
        //,EMBEDDINGS = 2
    }

    [JsonConverter(typeof(DataJsonConverter))]
    public class Data
    {
        public DataFormat Format { get; } = DataFormat.RAW;

        // Raw data is just a string
        public string? Raw { get; }

        // Structured data is key/value pairs        
        public Dictionary<string, Data>? Structured { get; }

        /*
        // Embeddings data is model-specific vector sets
        public Dictionary<string, Embedding>? Embeddings { get; }
        */
                
        public Data(string? raw, DataFormat dataFormat = DataFormat.RAW)            
        {
            Raw = raw;
            Format = dataFormat;

            if (dataFormat == DataFormat.STRUCTURED)
            {   
                Structured = raw == null ? new Dictionary<string, Data>() : JsonSerializer.Deserialize<Dictionary<string, Data>>(raw);
            }
        }

        public Data(Dictionary<string, Data> structured)
        {
            Format = DataFormat.STRUCTURED;
            Structured = structured;
            Raw = JsonSerializer.Serialize(structured);
        }
        /*
        public Data(Embedding embedding)
        {
            DataFormat = DataFormat.EMBEDDINGS;

            if (Embeddings == null)
            {
                Embeddings = new Dictionary<string, Embedding>();
            }

            Embeddings.Add(embedding.ModelId, embedding);
        }*/

        public override string? ToString() => Raw;        

        public static implicit operator Data(string? raw) => new Data(raw);

        public static implicit operator Data(Dictionary<string, Data> structured) => new Data(structured);

        //public static implicit operator Data(Embedding embedding) => new Data(embedding);

        public static implicit operator string?(Data data) => data.Raw;

        //public static implicit operator Dictionary<string, Embedding>?(Data data) => data.Embeddings;

        public static implicit operator Dictionary<string,Data>?(Data data) => data.Structured;
    }
}
