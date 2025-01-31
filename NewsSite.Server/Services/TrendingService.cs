using Microsoft.Extensions.Caching.Memory;
using NewsSite.Server.Models;
using NewsSite.Server.Models.ArticleAggregate;
using System.Text.Json;

namespace NewsSite.Server.Services
{
    public interface ITrendingService
    {
        Task<TrendingTopics> GetTrendingTopicsAsync();
    }
     
    public class TrendingService : ITrendingService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private const string CACHE_KEY = "trending_topics";
        private const int CACHE_DURATION_MINUTES = 30;

        public TrendingService(
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<TrendingTopics> GetTrendingTopicsAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY, out TrendingTopics cachedTopics))
            {
                return cachedTopics;
            }

            var topics = await FetchTrendingTopicsAsync();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

            _cache.Set(CACHE_KEY, topics, cacheOptions);
            return topics;
        }

        private async Task<TrendingTopics> FetchTrendingTopicsAsync()
        {
            var twitterTopics = await FetchTwitterTrendsAsync();
            var redditTopics = await FetchRedditTrendsAsync();

            return new TrendingTopics
            {
                TwitterTopics = twitterTopics,
                RedditTopics = redditTopics
            };
        }

        private async Task<List<SocialMediaTopic>> FetchTwitterTrendsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["Twitter:BearerToken"]}");

                var response = await client.GetAsync("https://api.twitter.com/2/trends/place?id=1");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Twitter API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                var trends = data[0].GetProperty("trends");

                return trends.EnumerateArray()
                    .Where(t => t.GetProperty("tweet_volume").GetInt32() > 0)
                    .Take(5)
                    .Select(t => new SocialMediaTopic
                    {
                        Title = t.GetProperty("name").GetString()!,
                        Url = t.GetProperty("url").GetString()!
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Fallback to default topics if API fails
                return new List<SocialMediaTopic>
                {
                    new() { Title = "#Technology", Url = "https://twitter.com/search?q=%23Technology" },
                    new() { Title = "#AI", Url = "https://twitter.com/search?q=%23AI" },
                    new() { Title = "#Crypto", Url = "https://twitter.com/search?q=%23Crypto" },
                    new() { Title = "#Innovation", Url = "https://twitter.com/search?q=%23Innovation" },
                    new() { Title = "#Future", Url = "https://twitter.com/search?q=%23Future" }
                };
            }
        }

        private async Task<List<SocialMediaTopic>> FetchRedditTrendsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://www.reddit.com/r/popular/hot.json");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Reddit API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                var posts = data.GetProperty("data").GetProperty("children");

                return posts.EnumerateArray()
                    .Take(5)
                    .Select(p =>
                    {
                        var subreddit = p.GetProperty("data").GetProperty("subreddit").GetString()!;
                        return new SocialMediaTopic
                        {
                            Title = $"r/{subreddit}",
                            Url = $"https://reddit.com/r/{subreddit}"
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Fallback to default topics if API fails
                return new List<SocialMediaTopic>
                {
                    new() { Title = "r/technology", Url = "https://reddit.com/r/technology" },
                    new() { Title = "r/worldnews", Url = "https://reddit.com/r/worldnews" },
                    new() { Title = "r/science", Url = "https://reddit.com/r/science" },
                    new() { Title = "r/futurology", Url = "https://reddit.com/r/futurology" },
                    new() { Title = "r/space", Url = "https://reddit.com/r/space" }
                };
            }
        }
    }
} 