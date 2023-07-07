namespace Technologai
{
    public class Data
    {
        public string? Unstructured { get; set; }
        public Dictionary<string, string>? Structured { get; set; }
        public float[][]? Embeddings { get; set; }

        public Data() { }

        public Data(string unstructured)
        {
            Unstructured = unstructured;
        }

        public Data(Dictionary<string, string> structured)
        {
            Structured = structured;
        }

        public Data(float[][] embeddings)
        {
            Embeddings = embeddings;
        }
    }
}
