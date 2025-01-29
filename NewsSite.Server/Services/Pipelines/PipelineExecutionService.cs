using NewsSite.Server.Data;
using NewsSite.Server.Models;
using NewsSite.Server.Services.Pipeline;

namespace NewsSite.Server.Services.Pipelines
{
    public interface IPipelineExecutionService
    {
        Task<PipelineResult> ExecutePipelineAsync(string pipelineId, Dictionary<string, object> initialInputs, CancellationToken cancellationToken = default);
    }

    public class PipelineExecutionService : IPipelineExecutionService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<PipelineExecutionService> _logger;
        private readonly IMetricsService _metricsService;
        private readonly IEnumerable<IStep> _steps;

        public PipelineExecutionService(
            IApplicationDbContext dbContext,
            ILogger<PipelineExecutionService> logger,
            IMetricsService metricsService,
            IEnumerable<IStep> steps)
        {
            _dbContext = dbContext;
            _logger = logger;
            _metricsService = metricsService;
            _steps = steps;
        }

        public async Task<PipelineResult> ExecutePipelineAsync(
            string pipelineId,
            Dictionary<string, object> initialInputs,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting pipeline execution for {PipelineId}", pipelineId);
                var pipeline = await _dbContext.GetPipelineAsync(pipelineId);
                
                if (pipeline == null)
                {
                    throw new ArgumentException($"Pipeline {pipelineId} not found");
                }

                var context = new Dictionary<string, object>(initialInputs);
                var results = new List<StepResult>();

                // Execute SERP API step
                var serpStep = _steps.FirstOrDefault(s => s.Name == "SerpAPI Processing");
                if (serpStep != null)
                {
                    var serpResult = await ExecuteStepAsync(serpStep, context);
                    results.Add(serpResult);
                    MergeResults(context, serpResult.Outputs);
                }

                // Execute Fact Extraction step
                var factExtractionStep = _steps.FirstOrDefault(s => s.Name == "Fact Extraction");
                if (factExtractionStep != null)
                {
                    var factResult = await ExecuteStepAsync(factExtractionStep, context);
                    results.Add(factResult);
                    MergeResults(context, factResult.Outputs);
                }

                // Execute Title Generation step
                var titleStep = _steps.FirstOrDefault(s => s.Name == "Title Generation");
                if (titleStep != null)
                {
                    var titleResult = await ExecuteStepAsync(titleStep, context);
                    results.Add(titleResult);
                    MergeResults(context, titleResult.Outputs);
                }

                return new PipelineResult
                {
                    PipelineId = pipelineId,
                    Success = results.All(r => r.Success),
                    StepResults = results,
                    FinalOutputs = context
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        private async Task<StepResult> ExecuteStepAsync(IStep step, Dictionary<string, object> inputs)
        {
            try
            {
                _logger.LogInformation("Executing step {StepName}", step.Name);
                var startTime = DateTime.UtcNow;
                
                var outputs = await step.ExecuteAsync(inputs);
                
                var duration = DateTime.UtcNow - startTime;

                return new StepResult
                {
                    StepName = step.Name,
                    Success = true,
                    Outputs = outputs,
                    Duration = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing step {StepName}", step.Name);
                return new StepResult
                {
                    StepName = step.Name,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private void MergeResults(Dictionary<string, object> context, Dictionary<string, object> outputs)
        {
            foreach (var output in outputs)
            {
                context[output.Key] = output.Value;
            }
        }
    }

    public class StepResult
    {
        public string StepName { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, object> Outputs { get; set; } = new();
        public string Error { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class PipelineResult
    {
        public string PipelineId { get; set; }
        public bool Success { get; set; }
        public List<StepResult> StepResults { get; set; } = new();
        public Dictionary<string, object> FinalOutputs { get; set; } = new();
    }
}