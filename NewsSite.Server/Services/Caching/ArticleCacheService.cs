using Microsoft.Extensions.Caching.Memory;
using NewsSite.Server.Models;

namespace NewsSite.Server.Services.Caching
{
    public interface IArticleCacheService
    {
        Task<List<ProcessedArticle>> GetOrCreateArticlesAsync(int pipelineId, Func<Task<List<ProcessedArticle>>> factory);
        void InvalidateCache(int pipelineId);
    }

    public class ArticleCacheService : IArticleCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ArticleCacheService> _logger;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public ArticleCacheService(IMemoryCache cache, ILogger<ArticleCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        public async Task<List<ProcessedArticle>> GetOrCreateArticlesAsync(int pipelineId, Func<Task<List<ProcessedArticle>>> factory)
        {
            string cacheKey = $"pipeline_{pipelineId}_articles";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetOptions(_cacheOptions);
                return await factory();
            });
        }

        public void InvalidateCache(int pipelineId)
        {
            string cacheKey = $"pipeline_{pipelineId}_articles";
            _cache.Remove(cacheKey);
        }
    }
}