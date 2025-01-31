using Microsoft.Extensions.Logging;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Pipeline.Steps;
using NewsSite.Server.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Server.Services.Pipeline.Utils;
using NewsSite.Server.Services.Pipeline.ArrayProcessing;

namespace NewsSite.Server.Services.Pipeline
{
    public class PipelineExecutionService : IPipelineExecutionService
    {
        private readonly IPipelineService _pipelineService;
        private readonly ILogger<PipelineExecutionService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _stepTypes;
        private const int DefaultMaxRetries = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        public PipelineExecutionService(
            IPipelineService pipelineService,
            ILogger<PipelineExecutionService> logger,
            IServiceProvider serviceProvider)
        {
            _pipelineService = pipelineService;
            _logger = logger;
            _serviceProvider = serviceProvider;

            _stepTypes = new Dictionary<string, Type>
            {
                { "openai.completion", typeof(OpenAICompletionStep) },
                { "serp.search", typeof(SerpApiStep) },
                { "database.operation", typeof(DatabaseStep) },
                 { "news.category", typeof(NewsCategoryStep) }
            };
        }

        public async Task<Dictionary<string, object>> ExecutePipelineAsync(string pipelineId, Dictionary<string, object> initialInput = null)
        {
            var pipeline = await _pipelineService.GetPipelineByIdAsync(pipelineId);
            if (pipeline == null)
                throw new ArgumentException($"Pipeline {pipelineId} not found");

            var context = new PipelineExecutionContext(pipelineId, pipeline.Steps.Count);

            try
            {
                var currentInput = initialInput ?? new Dictionary<string, object>();

                switch (pipeline.Mode)
                {
                    case PipelineMode.Sequential:
                        await ExecuteSequentialStepsAsync(pipeline, context, currentInput);
                        break;

                    case PipelineMode.Parallel:
                        await ExecuteParallelStepsAsync(pipeline, context, currentInput);
                        break;

                    case PipelineMode.Conditional:
                        await ExecuteConditionalStepsAsync(pipeline, context, currentInput);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported pipeline mode: {pipeline.Mode}");
                }

                context.AddLog("system", "Pipeline execution completed successfully", LogLevel.Information);
                context.UpdateStepState("system", StepExecutionState.Completed);
            }
            catch (OperationCanceledException)
            {
                context.AddLog("system", "Pipeline execution was cancelled", LogLevel.Warning);
                context.UpdateStepState("system", StepExecutionState.Cancelled);
            }
            catch (Exception ex)
            {
                context.AddLog("system", $"Pipeline execution failed: {ex.Message}", LogLevel.Error);
                context.UpdateStepState("system", StepExecutionState.Failed);
                _logger.LogError(ex, "Pipeline execution failed");
                throw;
            }

            return context.GetExecutionState();
        }

        private async Task ExecuteSequentialStepsAsync(PipelineModel pipeline, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            var currentInput = input;

            foreach (var step in pipeline.Steps.OrderBy(s => s.Order))
            {
                await ExecuteStepWithRetryAsync(step, context, currentInput);
                
                if (context.Status == PipelineExecutionStatus.Failed || 
                    context.Status == PipelineExecutionStatus.Cancelled)
                    break;

                if (step.PassOutputAsInput)
                {
                    currentInput = context.GetStepOutput(step.Id);
                }
            }
        }

        private async Task ExecuteParallelStepsAsync(PipelineModel pipeline, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            var tasks = pipeline.Steps
                .Select(step => ExecuteStepWithRetryAsync(step, context, input))
                .ToList();

            await Task.WhenAll(tasks);
        }

        private async Task ExecuteConditionalStepsAsync(PipelineModel pipeline, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            var currentInput = input;
            var executedSteps = new HashSet<string>();

            while (true)
            {
                var nextStep = pipeline.Steps
                    .Where(s => !executedSteps.Contains(s.Id))
                    .FirstOrDefault(s => 
                        string.IsNullOrEmpty(s.Condition) || 
                        context.EvaluateCondition(s.Condition));

                if (nextStep == null) break;

                await ExecuteStepWithRetryAsync(nextStep, context, currentInput);
                executedSteps.Add(nextStep.Id);

                if (context.Status == PipelineExecutionStatus.Failed || 
                    context.Status == PipelineExecutionStatus.Cancelled)
                    break;

                if (nextStep.PassOutputAsInput)
                {
                    currentInput = context.GetStepOutput(nextStep.Id);
                }
            }
        }

        private async Task ExecuteStepWithRetryAsync(PipelineStep step, PipelineExecutionContext context, Dictionary<string, object> input)
        {
            context.UpdateStepState(step.Id, StepExecutionState.Pending);

            if (!_stepTypes.TryGetValue(step.Type, out var stepType))
            {
                throw new InvalidOperationException($"Unknown step type: {step.Type}");
            }

            var stepInstance = ActivatorUtilities.CreateInstance(_serviceProvider, stepType) as IPipelineStep;
            if (stepInstance == null)
            {
                throw new InvalidOperationException($"Failed to create step instance for type: {step.Type}");
            }

            context.AddLog(step.Id, $"Starting step execution: {step.Name}");
            context.UpdateStepState(step.Id, StepExecutionState.Running);

            var maxRetries = step.MaxRetries ?? DefaultMaxRetries;

            try
            {
                Dictionary<string, object> stepOutput;

                if (step.Parameters.Any(param => param.Value.Contains("[*]")))
                {
                    var arrayParam = step.Parameters.First(param => param.Value.Contains("[*]"));
                    var processor = new ArrayProcessor(stepInstance, context, maxRetries);
                    
                    stepOutput = await processor.ProcessArrayParameter(
                        arrayParam.Key,
                        arrayParam.Value,
                        input,
                        step.Parameters
                    );
                }
                else
                {
                    // Normal execution for non-list parameters
                    stepOutput = await ExecuteWithRetryAsync(stepInstance, context, input, step.Parameters, maxRetries);
                }

                context.AddStepOutput(step.Id, stepOutput);
                context.AddLog(step.Id, "Step completed successfully");
                context.AddToExecutionPath(step.Id);
            }
            catch (Exception ex)
            {
                context.UpdateStepState(step.Id, StepExecutionState.Failed);
                context.AddLog(step.Id, $"Step failed: {ex.Message}", LogLevel.Error);
                throw new StepExecutionException(step.Id, $"Step execution failed: {ex.Message}", ex);
            }
        }

        private async Task<Dictionary<string, object>> ExecuteWithRetryAsync(
            IPipelineStep stepInstance,
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters,
            int maxRetries)
        {
            var attempt = 0;
            while (true)
            {
                try
                {
                    return await stepInstance.ExecuteAsync(context, input, parameters);
                }
                catch (Exception ex) when (++attempt <= maxRetries)
                {
                    context.AddLog(context.ExecutionId, $"Attempt {attempt}/{maxRetries} failed: {ex.Message}", LogLevel.Warning);
                    await Task.Delay(RetryDelay * attempt);
                }
            }
        }
    }
} 