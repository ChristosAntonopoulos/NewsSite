using Microsoft.Extensions.Logging;
using NewsSite.Server.Services.News;
using NewsSite.Server.Services.Pipeline.Utils;
using NewsSite.Server.Models.Pipeline;
using NewsSite.Server.Services.Pipeline;
using NewsSite.Server.Models.PipelineAggregate;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class NewsCategoryStep : BaseStep
    {
        private readonly INewsCategoryService _newsCategoryService;
        private readonly ILogger<NewsCategoryStep> _logger;

        public override string StepType => "news.category";

        public NewsCategoryStep(
            INewsCategoryService newsCategoryService,
            ILogger<NewsCategoryStep> logger) : base(logger)
        {
            _newsCategoryService = newsCategoryService;
            _logger = logger;
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            try
            {
                ValidateRequiredParameter("category", parameters, context.ExecutionId);
                ValidateRequiredParameter("outputPath", parameters, context.ExecutionId);

                var category = ReplaceVariables(parameters["category"], context, input);
                
                _logger.LogInformation($"Executing news category step for category: {category}");
                
                var result = await _newsCategoryService.GetNewsByCategory(category);

                var output = new Dictionary<string, object>
                {
                    { "category", category },
                    { "results", result.InitialResults }
                };

                SetOutputValue(context, parameters, output);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing news category step");
                throw new StepExecutionException(context.ExecutionId, "News category step failed", ex);
            }
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "category", "outputPath" };
        }
    }
} 