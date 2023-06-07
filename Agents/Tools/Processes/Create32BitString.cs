namespace Technologai
{
    public class Generate32BitString : Process
    {   
        public new string? Name { get; set; } = "Generate 32 Bit String";
        public new string Description { get; set; } = "Generate a randomized 32-bit string.";
        public new string SampleJsonOut { get; set; } = "{\"content\":\"string\"}";

        public new Task<ExecuteResult> Execute(Assessment assessment)
        {
            return Task.FromResult(new ExecuteResult
            {   
                Output = Utils.GenerateNewIdString(32)
            });
        }
    }
}