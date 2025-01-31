using System.Text.Json;
using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.Pipeline;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.SerpAi;
using Microsoft.Extensions.Logging;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class SerpApiStep : BaseStep
    {
        private readonly ISerpApiService _serpApiService;
        private readonly ILogger<SerpApiStep> _logger;

        public override string StepType => "serp.search";

        public SerpApiStep(
            ISerpApiService serpApiService,
            ILogger<SerpApiStep> logger) : base(logger)
        {
            _serpApiService = serpApiService;
            _logger = logger;
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(PipelineExecutionContext context, Dictionary<string, object> input, Dictionary<string, string> parameters)
        {
            try
            {
                ValidateRequiredParameter("query", parameters, context.ExecutionId);

                var query = parameters["query"];
                var engine = GetTypedParameter(parameters, "engine", "google");
                var maxResults = GetTypedParameter(parameters, "maxResults", 10);
                var includeImages = GetTypedParameter(parameters, "includeImages", false);

                var searchParams = new SearchParameters
                {
                    Engine = engine,
                    Num = maxResults,
                    IncludeImages = includeImages
                };

                if (parameters.TryGetValue("additionalParams", out var additionalParamsJson))
                {
                    searchParams.AdditionalParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(additionalParamsJson);
                }

                var searchResults = await _serpApiService.SearchAsync(query, searchParams);

                var result = new Dictionary<string, object>
                {
                    ["data"] = searchResults,
                    ["metadata"] = new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTime.UtcNow,
                        ["query"] = query,
                        ["engine"] = engine,
                        ["maxResults"] = maxResults
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SERP API step");
                throw new StepExecutionException(context.ExecutionId, "SERP API search failed", ex);
            }
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "query" };
        }
    }
} 