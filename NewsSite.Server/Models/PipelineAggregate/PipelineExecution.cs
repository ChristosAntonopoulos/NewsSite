using NewsSite.Server.Models;

public class PipelineExecution
{
    public int Id { get; set; }
    public int PipelineId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } // "Running", "Completed", "Failed"
    public string ErrorMessage { get; set; }
    public NewsPipeline Pipeline { get; set; }
} 