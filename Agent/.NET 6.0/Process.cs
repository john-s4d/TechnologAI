using System.Text.Json;

namespace Technologai
{
    public class Process : IProcess
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string? Prompt { get; set; }
        public string SampleJsonIn { get; set; }
        public string SampleJsonOut { get; set; }
        public string? MemberId { get; set; }

        public Process(string id, string description, string sampleJsonIn, string sampleJsonOut)
        {
            Id = id;
            Description = description;
            SampleJsonIn = sampleJsonIn;
            SampleJsonOut = sampleJsonOut;
        }

        public virtual Task<Assessment> Assess(InformationAdapter information)
        {
            return Task.FromResult(information.Assessment);
        }

        public virtual Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            return Task.FromResult(data);
        }

        public virtual Task<List<Information>> Spawn(InformationAdapter information)
        {
            return Task.FromResult(new List<Information>());
        }

        public static Process? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Process>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}