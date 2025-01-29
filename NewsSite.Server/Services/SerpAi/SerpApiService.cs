using Microsoft.Extensions.Options;
using NewsSite.Server.Configuration;
using NewsSite.Server.Models.Pipeline;
using System.Text.Json;
using System.Linq;

namespace NewsSite.Server.Services.SerpAi
{
    public class SerpApiService : ISerpApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<SerpApiService> _logger;

        public SerpApiService(IOptions<SerpApiSettings> settings, HttpClient httpClient, ILogger<SerpApiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://serpapi.com/");
        }

        public async Task<Dictionary<string, object>> SearchAsync(string query, SearchParameters parameters = null)
        {
            try
            {
                parameters ??= new SearchParameters();
                var requestUrl = BuildRequestUrl(query, parameters);
                
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                // Ensure we only return the requested number of results
                if (results.TryGetValue("organic_results", out var organicResults) && 
                    organicResults is JsonElement organicArray && 
                    organicArray.ValueKind == JsonValueKind.Array)
                {
                    var limitedResults = organicArray.EnumerateArray()
                        .Take(parameters.Num ?? 10)
                        .ToList();

                    results["organic_results"] = limitedResults;
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SERP API search for query: {Query}", query);
                throw;
            }
        }

        private string BuildRequestUrl(string query, SearchParameters parameters)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["q"] = Uri.EscapeDataString(query),
                ["num"] = (parameters.Num ?? 10).ToString(),
                ["api_key"] = _apiKey,
                ["engine"] = parameters.Engine ??   "google",
                ["tbm"] = "nws",
                ["start"] = "1"
            };

            if (parameters.IncludeImages)
            {
                queryParams["tbm"] = "isch";
            }

            if (parameters.AdditionalParameters != null)
            {
                foreach (var param in parameters.AdditionalParameters)
                {
                    queryParams[param.Key] = param.Value;
                }
            }

            _logger.LogInformation($"Building request URL with num={parameters.Num}, tbm=nws");
            return $"search.json?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }
    }
} 