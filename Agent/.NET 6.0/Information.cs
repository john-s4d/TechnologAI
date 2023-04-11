using System.Management;
using System.Text.Json;

namespace Technologai
{
    public enum InformationState
    {
        OPEN, // Ready to start as soon as we can find a worker
        WORKING, // Actively working on it by this worker
        DELEGATED, // Open and worker has passed parts down to other workers        
        INCOMPLETE, // Stays with the worker. Might be completed in the future.        
        COMPLETE, // Done and returning or with Creator
        ANALYSIS, // Rating, training, etc..
        CLOSED // Archive/d
    }

    public class Information
    {
        public ContextId ContextId { get; }
        public string CreatorId { get; }
        public InformationState State { get; set; }
        public string? Input { get; set; }
        public string? SchemaIn { get; set; }
        public string? Output { get; set; }
        public string? SchemaOut { get; set; }
        public string? Feedback { get; set; }


        // TODO History, Signatures, ReadOnly fields

        public Information(ContextId contextId, string creatorId, InformationState state = InformationState.OPEN)
        {
            ContextId = contextId;
            CreatorId = creatorId;
            State = state;
        }
        public static Information? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Information>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public void Complete(string output)
        {
            Output = output;
            State = InformationState.COMPLETE;
        }
    }
}
