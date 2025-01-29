using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Pipeline.Utils;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class YouTubeStep : BaseStep
    {
        private readonly ILogger<YouTubeStep> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private object _baseUrl;

        public override string StepType => "youtube.operation";

        public YouTubeStep(
            ILogger<YouTubeStep> logger,
            HttpClient httpClient,
            IOptions<YouTubeSettings> settings) : base(logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            _httpClient.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            var operation = parameters.GetValueOrDefault("operation", "search");
            var maxResults = int.Parse(parameters.GetValueOrDefault("maxResults", "50"));
            var order = parameters.GetValueOrDefault("order", "date");
            var type = parameters.GetValueOrDefault("type", "video");

            try
            {
                if (operation == "search" && !parameters.ContainsKey("query"))
                {
                    throw new ArgumentNullException("query", "Search query cannot be null");
                }

                var result = operation switch
                {
                    "search" => await SearchVideos(parameters.GetValueOrDefault("query"), maxResults, order, type),
                    "channelVideos" => await GetChannelVideos(parameters.GetValueOrDefault("channelId"), maxResults, order),
                    "trending" => await GetTrendingVideos(maxResults, type),
                    "comments" => await GetVideoComments(parameters.GetValueOrDefault("videoId"), maxResults),
                    _ => throw new ArgumentException($"Unsupported operation: {operation}")
                };

                return ApplyTransformations(new Dictionary<string, object>
                {
                    { "result", result }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing YouTube operation: {Operation}", operation);
                throw;
            }
        }

        private string BuildSearchEndpoint(string query, Dictionary<string, string> queryParams)
        {
            queryParams["q"] = Uri.EscapeDataString(query);
            return $"search?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildChannelVideosEndpoint(string channelId, Dictionary<string, string> queryParams)
        {
            queryParams["channelId"] = channelId;
            return $"search?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildTrendingEndpoint(Dictionary<string, string> queryParams)
        {
            queryParams["chart"] = "mostPopular";
            return $"videos?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildCommentsEndpoint(string videoId, Dictionary<string, string> queryParams)
        {
            queryParams["videoId"] = videoId;
            return $"commentThreads?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "operation", "outputPath" };
        }

        private async Task<object> SearchVideos(string query, int maxResults, string order, string type)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["q"] = query,
                ["part"] = "snippet,statistics",
                ["maxResults"] = maxResults.ToString(),
                ["type"] = type,
                ["order"] = order,
                ["key"] = _apiKey
            };

            var response = await _httpClient.GetAsync(BuildUrl("search", queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetChannelVideos(string channelId, int maxResults, string order)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["channelId"] = channelId,
                ["part"] = "snippet,statistics",
                ["maxResults"] = maxResults.ToString(),
                ["order"] = order,
                ["type"] = "video",
                ["key"] = _apiKey
            };

            var response = await _httpClient.GetAsync(BuildUrl("search", queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetTrendingVideos(int maxResults, string type)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["part"] = "snippet,statistics",
                ["maxResults"] = maxResults.ToString(),
                ["chart"] = "mostPopular",
                ["regionCode"] = "US",
                ["videoCategoryId"] = GetVideoCategoryId(type),
                ["key"] = _apiKey
            };

            var response = await _httpClient.GetAsync(BuildUrl("videos", queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetVideoComments(string videoId, int maxResults)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["videoId"] = videoId,
                ["part"] = "snippet",
                ["maxResults"] = maxResults.ToString(),
                ["order"] = "relevance",
                ["key"] = _apiKey
            };

            var response = await _httpClient.GetAsync(BuildUrl("commentThreads", queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private string BuildUrl(string endpoint, Dictionary<string, string> queryParams)
        {
            var query = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{_baseUrl}/{endpoint}?{query}";
        }

        private async Task<object> ProcessResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(content);
        }

        private string GetVideoCategoryId(string type)
        {
            return type switch
            {
                "music" => "10",
                "gaming" => "20",
                "news" => "25",
                "sports" => "17",
                _ => "0" // Default category (Film & Animation)
            };
        }
    }
} 