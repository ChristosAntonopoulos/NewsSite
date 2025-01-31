using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Services.Pipeline
{
    public interface IPipelineStep
    {
        string StepType { get; }
        Task<Dictionary<string, object>> ExecuteAsync(PipelineExecutionContext context, Dictionary<string, object> input, Dictionary<string, string> parameters);
        Task<bool> ValidateAsync(Dictionary<string, string> parameters);
        IEnumerable<string> GetRequiredParameters();
    }

    public class StepExecutionException : Exception
    {
        public string StepId { get; }

        public StepExecutionException(string stepId, string message, Exception innerException = null) 
            : base(message, innerException)
        {
            StepId = stepId;
        }
    }
} 