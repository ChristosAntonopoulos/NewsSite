using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using NewsSite.Server.Services.Pipeline;
using NewsSite.Server.Models.SearchSettings;

namespace NewsSite.Server.Services.SerpAi
{
    public class SerpAiProcessingService : IStep
    {
        private readonly CustomSearchAPIService _searchService;
        private readonly ILogger<SerpAiProcessingService> _logger;
        private readonly SerpApiSettings _settings;

        public string Name => "SerpAPI Processing";

        public SerpAiProcessingService(
            ILogger<SerpAiProcessingService> logger,
            string apiKey,
            string searchEngineId,
            SerpApiSettings settings)
        {
            _logger = logger;
            _settings = settings;

            _searchService = new CustomSearchAPIService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "NewsSite"
            });
        }

        public async Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs)
        {
            try
            {
                _logger.LogInformation("Starting SerpAPI search");

                if (!inputs.TryGetValue("query", out var queryObj))
                {
                    throw new ArgumentException("Search query not provided");
                }

                var query = queryObj.ToString();
        var searchResults = await PerformSearch(query, _settings.SearchType, _settings.ResultsPerPage);

                return new Dictionary<string, object>
                {
                    { "results", searchResults.Items },
                    { "total_results", searchResults.SearchInformation?.TotalResults ?? "0" },
            { "search_time", searchResults.SearchInformation?.SearchTime ?? 0.0 }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SerpAPI processing");
                throw;
            }
        }


        private async Task<Google.Apis.CustomSearchAPI.v1.Data.Search> PerformSearch(string query, string engine, int maxResults)
        {
            var searchRequest = _searchService.Cse.List();
            searchRequest.Q = query;
            searchRequest.Cx = _settings.SearchEngineId;
            searchRequest.Num = maxResults;
            searchRequest.Start = _settings.StartIndex;
            
            // Set search type from engine parameter
            if (!string.IsNullOrEmpty(engine))
            {
                if (Enum.TryParse<CseResource.ListRequest.SearchTypeEnum>(engine, true, out var searchType))
                {
                    searchRequest.SearchType = searchType;
                }
                else
                {
                    _logger.LogWarning($"Invalid search type: {engine}");
                }
            }
            
            if (!string.IsNullOrEmpty(_settings.Language))
            {
                searchRequest.Lr = _settings.Language;
            }

            _logger.LogInformation($"Executing search with query: {query}, searchType: {engine}, maxResults: {maxResults}");
            return await searchRequest.ExecuteAsync();
        }
    }
}
