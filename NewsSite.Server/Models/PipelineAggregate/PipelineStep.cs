using MongoDB.Bson.Serialization.Attributes;

namespace NewsSite.Server.Models.PipelineAggregate
{
    public class PipelineStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
        public bool PassOutputAsInput { get; set; }
        
        // Optional properties for enhanced execution
        public int? MaxRetries { get; set; }
        public string? Condition { get; set; } = null;
        public bool IgnoreErrors { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Dictionary<string, string>? RetryConditions { get; set; } = null;
        public List<string> DependsOn { get; set; } = new();

        public PipelineStep()
        {
            Parameters = new Dictionary<string, string>();
            DependsOn = new List<string>();
            IgnoreErrors = false;
            PassOutputAsInput = false;
        }
    }
    

    public enum PipelineMode
    {
        Sequential,
        Parallel,
        Conditional
    }
} 