using Microsoft.Extensions.Caching.Memory;
using TaskOption;

namespace Library.Service.CacheService
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

        public async Task<T?> UpdateCache<T>(CacheUpdate<T> options)
        {
            if (_cache.TryGetValue(options.NomeCache, out T? currentValue) && currentValue != null)
            {
                T? updatedValue = await options.Task(currentValue);
                _cache.Set(options.NomeCache, updatedValue, options.DurataCache);
                return updatedValue;
            }

            return default;
        }
    }
}
