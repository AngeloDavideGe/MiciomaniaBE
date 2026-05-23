using Microsoft.Extensions.Caching.Memory;
using TaskOption;

namespace CacheName
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrCreate<T>(CacheOptions<T> options)
        {
            return (await _cache.GetOrCreateAsync(options.NomeCache, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = options.DurataCache;
                return await options.Task();
            }))!;
        }
    }
}
