using Technologai;

public interface IExecute
{
    public string Description { get; }
    public string SampleJsonIn { get; }
    public string SampleJsonOut { get; }
    public Task<Dictionary<string, object>> Execute(Dictionary<string,object> data);    
}