using Microsoft.VisualBasic;

namespace Technologai.Agents
{
    internal class DisplayLogMessage : Process
    {
        internal event Action<string>? LogMessage;
        public new string Id { get; set; } = "display_log_message";
        public new string Description => "Display a message on the output log screen.";
        public new string SampleJsonIn => "{\"message\":\"string\",\"count\":\"string\"}";

        public new Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {
            LogMessage?.Invoke((string)data["message"]);
            return Task.FromResult(data);
        }

        public new Task<Assessment> Assess(InformationAdapter information)
        {
            Assessment assessment = new Assessment();
            assessment.Result = AssessmentResult.EXECUTE;
            return Task.FromResult(assessment);
        }

        public new Task<List<Information>> Spawn(InformationAdapter information)
        {
            throw new NotImplementedException();
        }
    }
}
