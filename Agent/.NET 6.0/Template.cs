namespace Technologai
{
    public enum TemplateState
    {
        RESTING = 0,
        ASSESSING = 1,
        PROCESSING = 2        
    }
    
    public interface ITemplate
    {
        public string? Id { get; set; }
        //public string? Description { get; set; }
        public Data? Description { get; set; }
        public string[]? InputKeys { get; set; }
        public string[]? OutputKeys { get; set; }
        public string? MemberId { get; set; }

        public abstract Task<bool> Assess(InformationAdapter information);
        public abstract Task<Data?> Process(InformationAdapter information);
    }

    public class Template : ITemplate
    {
        public string? Id { get; set; }
        //public string? Description { get; set; }
        public Data? Description { get; set; }
        public string[]? InputKeys { get; set; }
        public string[]? OutputKeys { get; set; }
        public string? MemberId { get; set; }

        // NOTE: We don't want these methods to set the information properties. This way they can be inspected by the calling code before the data is committed.
        public virtual Task<bool> Assess(InformationAdapter information) => Task.FromResult(false);
        public virtual Task<Data?> Process(InformationAdapter information) => Task.FromResult((Data?)null);

    }
}