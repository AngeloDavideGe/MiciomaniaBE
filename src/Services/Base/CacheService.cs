using Microsoft.Extensions.Caching.Memory;

public class CacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreate<T>(
        string key,
        TimeSpan duration,
        Func<Task<T>> factory)
    {
        return (await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = duration;
            return await factory();
        }))!;
    }
}