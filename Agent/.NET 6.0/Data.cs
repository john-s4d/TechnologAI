namespace Technologai
{
    public class Data
    {
        // Unstructured data is just a string
        public string? Unstructured { get; set; }

        // Structured data is key/value pairs
        public Dictionary<string, string>? Structured { get; set; }

        // Embeddings data is model-specific vector sets
        public Dictionary<string, Embedding>? Embeddings { get; set; }

        public Data() { }

        public Data(string unstructured)
        {
            Unstructured = unstructured;
        }

        public static implicit operator Data(string unstructured)
        {
            return new Data(unstructured);
        }

        public Data(Dictionary<string, string> structured)
        {
            Structured = structured;
        }

        public static implicit operator Data(Dictionary<string, string> structured)
        {
            return new Data(structured);
        }

        public Data(Embedding embedding)
        {
            if (Embeddings == null)
            {
                Embeddings = new Dictionary<string, Embedding>();
            }

            Embeddings.Add(embedding.ModelId, embedding);
        }

        public static implicit operator Data(Embedding embedding)
        {
            return new Data(embedding);
        }

        public override string? ToString()
        {
            return Unstructured ?? base.ToString(); // TODO: Convert structured and embeddings to string
        }
    }
}
