using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NewsSite.Server.Interfaces;

namespace NewsSite.Server.Services.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult(_cache.Get<T>(key));
        }

        public Task SetAsync<T>(string key, T value) where T : class
        {
            _cache.Set(key, value, _cacheOptions);
            return Task.CompletedTask;
        }
    }
}