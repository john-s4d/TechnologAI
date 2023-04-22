namespace Technologai
{
    public enum AssessmentResult
    {
        SPAWN,
        EXECUTE,
        HOLD
    }

    public class Assessment
    {
        public Dictionary<string, object> Data { get; private set;} = new Dictionary<string, object>();

        public string Summary { get; set; } = string.Empty;

        public AssessmentResult Result { get; set; } = AssessmentResult.HOLD;

    }
}
