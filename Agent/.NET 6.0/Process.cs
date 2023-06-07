namespace Technologai
{
    public class Process : IProcess
    {
        public string? Id { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;        
        public string? SampleJsonIn { get; set; }
        public string? SampleJsonOut { get; set; }
        public string? MemberId { get; set; }

        public Process() { }

        public Process(string id, string description, string sampleJsonIn, string sampleJsonOut)
        {
            Id = id;
            Description = description;
            SampleJsonIn = sampleJsonIn;
            SampleJsonOut = sampleJsonOut;
        }

        public virtual Task<Assessment> Assess(InformationAdapter information)
        {
            return Task.FromResult(new Assessment());
        }

        public Task<ExecuteResult> Execute(Assessment assessment)
        {
            return Task.FromResult(new ExecuteResult());
        }


        public virtual Task<List<Information>> Spawn(InformationAdapter information)
        {
            return Task.FromResult(new List<Information>());
        }


    }
}