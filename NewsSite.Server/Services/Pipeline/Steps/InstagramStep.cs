using System.Text.Json;
using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Models.Pipeline;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class InstagramStep : BaseStep
    {
        private readonly ILogger<InstagramStep> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private object _accessToken;
        private object _baseUrl;

        public override string StepType => "instagram.operation";

        public InstagramStep(
            ILogger<InstagramStep> logger,
            HttpClient httpClient,
            IOptions<InstagramSettings> settings) : base(logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            _httpClient.BaseAddress = new Uri("https://graph.instagram.com/v12.0/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            var operation = parameters.GetValueOrDefault("operation", "userPosts");
            var maxResults = int.Parse(parameters.GetValueOrDefault("maxResults", "50"));
            var includeStories = bool.Parse(parameters.GetValueOrDefault("includeStories", "false"));

            try
            {
                var result = operation switch
                {
                    "userPosts" => await GetUserPosts(parameters.GetValueOrDefault("username"), maxResults),
                    "hashtagPosts" => await GetHashtagPosts(parameters.GetValueOrDefault("hashtag"), maxResults),
                    "userStories" => includeStories ? await GetUserStories(parameters.GetValueOrDefault("username")) : new(),
                    "explore" => await GetExplorePosts(maxResults),
                    _ => throw new ArgumentException($"Unsupported operation: {operation}")
                };

                return ApplyTransformations(new Dictionary<string, object>
                {
                    { "result", result }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Instagram operation: {Operation}", operation);
                throw;
            }
        }

        private async Task<object> GetUserPosts(string username, int maxResults)
        {
            var endpoint = $"users/{username}/media";
            var queryParams = new Dictionary<string, string>
            {
                ["fields"] = "id,caption,media_type,media_url,permalink,thumbnail_url,timestamp,username",
                ["limit"] = maxResults.ToString()
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetHashtagPosts(string hashtag, int maxResults)
        {
            var endpoint = $"hashtags/{hashtag}/recent_media";
            var queryParams = new Dictionary<string, string>
            {
                ["fields"] = "id,caption,media_type,media_url,permalink,thumbnail_url,timestamp,username",
                ["limit"] = maxResults.ToString()
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetUserStories(string username)
        {
            var endpoint = $"users/{username}/stories";
            var queryParams = new Dictionary<string, string>
            {
                ["fields"] = "id,media_type,media_url,permalink,timestamp"
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetExplorePosts(int maxResults)
        {
            var endpoint = "discover/explore";
            var queryParams = new Dictionary<string, string>
            {
                ["fields"] = "id,caption,media_type,media_url,permalink,thumbnail_url,timestamp,username",
                ["limit"] = maxResults.ToString()
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private string BuildUrl(string endpoint, Dictionary<string, string> queryParams)
        {
            var query = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{_baseUrl}/{endpoint}?access_token={_accessToken}&{query}";
        }

        private async Task<object> ProcessResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(content);
        }

        private string BuildUserPostsEndpoint(string username, Dictionary<string, string> queryParams)
        {
            return $"users/{username}/media?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildHashtagPostsEndpoint(string hashtag, Dictionary<string, string> queryParams)
        {
            return $"tags/{hashtag}/media/recent?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildUserStoriesEndpoint(string username, Dictionary<string, string> queryParams)
        {
            return $"users/{username}/stories?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildExploreEndpoint(Dictionary<string, string> queryParams)
        {
            return $"explore/posts?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "operation" };
        }
    }
} 