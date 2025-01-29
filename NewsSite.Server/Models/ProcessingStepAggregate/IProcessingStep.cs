public interface IProcessingStep
{
    string PipelineId { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    int Order { get; set; }
    bool IsEnabled { get; set; }
    Task<ProcessingResult> ProcessAsync(Dictionary<string, object> inputs);
} 