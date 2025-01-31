namespace NewsSite.Server.Models
{
    public class PipelineResult
    {
        public string PipelineId { get; set; }
        public bool Success { get; set; }
        public List<ProcessingResult> StepResults { get; set; } = new();
        public Dictionary<string, object> Outputs { get; set; } = new();
        public string Error { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
} 