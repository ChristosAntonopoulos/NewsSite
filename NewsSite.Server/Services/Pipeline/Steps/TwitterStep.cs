using System.Text.Json;
using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Models.Pipeline;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class TwitterStep : BaseStep
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private object _baseUrl;

        public override string StepType => "twitter.operation";

        public TwitterStep(
            ILogger<TwitterStep> logger,
            HttpClient httpClient,
            IOptions<TwitterSettings> settings) : base(logger)
        {
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            _httpClient.BaseAddress = new Uri("https://api.twitter.com/2/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            var operation = parameters.GetValueOrDefault("operation", "search");
            var maxResults = int.Parse(parameters.GetValueOrDefault("maxResults", "100"));
            var includeRetweets = bool.Parse(parameters.GetValueOrDefault("includeRetweets", "false"));
            var language = parameters.GetValueOrDefault("language", "en");

            try
            {
                var result = operation switch
                {
                    "search" => await SearchTweets(parameters.GetValueOrDefault("query"), maxResults, includeRetweets, language),
                    "userTimeline" => await GetUserTimeline(parameters.GetValueOrDefault("username"), maxResults, includeRetweets),
                    "mentions" => await GetMentions(parameters.GetValueOrDefault("username"), maxResults),
                    "trends" => await GetTrends(parameters.GetValueOrDefault("location", "1")), // 1 is worldwide
                    _ => throw new ArgumentException($"Unsupported operation: {operation}")
                };

                return ApplyTransformations(new Dictionary<string, object>
                {
                    { "result", result }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Twitter operation: {Operation}", operation);
                throw;
            }
        }

        private string BuildSearchEndpoint(string query, Dictionary<string, string> queryParams)
        {
            queryParams["query"] = Uri.EscapeDataString(query);
            return $"tweets/search/recent?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildUserTweetsEndpoint(string username, Dictionary<string, string> queryParams)
        {
            return $"users/by/username/{username}/tweets?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }

        private string BuildTrendingEndpoint(string woeid)
        {
            return $"trends/place/{woeid}";
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return new[] { "operation" };
        }

        private async Task<object> SearchTweets(string query, int maxResults, bool includeRetweets, string language)
        {
            var endpoint = "tweets/search/recent";
            var queryParams = new Dictionary<string, string>
            {
                ["query"] = $"{query} lang:{language} {(includeRetweets ? "" : "-is:retweet")}",
                ["max_results"] = maxResults.ToString(),
                ["tweet.fields"] = "created_at,public_metrics,entities,context_annotations",
                ["expansions"] = "author_id,referenced_tweets.id",
                ["user.fields"] = "name,username,verified"
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetUserTimeline(string username, int maxResults, bool includeRetweets)
        {
            var endpoint = $"users/by/username/{username}/tweets";
            var queryParams = new Dictionary<string, string>
            {
                ["max_results"] = maxResults.ToString(),
                ["tweet.fields"] = "created_at,public_metrics,entities",
                ["exclude"] = includeRetweets ? "" : "retweets"
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetMentions(string username, int maxResults)
        {
            var endpoint = $"users/by/username/{username}/mentions";
            var queryParams = new Dictionary<string, string>
            {
                ["max_results"] = maxResults.ToString(),
                ["tweet.fields"] = "created_at,public_metrics,entities"
            };

            var response = await _httpClient.GetAsync(BuildUrl(endpoint, queryParams));
            response.EnsureSuccessStatusCode();
            return await ProcessResponse(response);
        }

        private async Task<object> GetTrends(string locationId)
        {
            var endpoint = $"trends/place/{locationId}";
            var response = await _httpClient.GetAsync(BuildUrl(endpoint, new Dictionary<string, string>()));
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
    }
} 