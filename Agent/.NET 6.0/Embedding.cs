namespace Technologai
{
    public class Embedding : List<float[]>
    {
        public string ModelId { get; set; }

        public Embedding(string modelId)
        {
            ModelId = modelId;
        }

        public Embedding(string modelId, IEnumerable<float[]> vectors) 
            : base(vectors)
        {
            ModelId = modelId;
        }        
    }
}
