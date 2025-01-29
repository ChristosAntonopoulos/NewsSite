public class ProcessingResult
{
    public string StepId { get; set; }
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public List<string> Warnings { get; set; } = new();

    public static ProcessingResult Complete(string stepId, string output, Dictionary<string, object> metadata = null) =>
        new()
        {
            StepId = stepId,
            Success = true,
            Output = output,
            ProcessedAt = DateTime.UtcNow,
            Metadata = metadata ?? new()
        };

    public static ProcessingResult Failure(string stepId, string error) =>
        new()
        {
            StepId = stepId,
            Success = false,
            Error = error,
            ProcessedAt = DateTime.UtcNow,
            Metadata = new()
        };
} 