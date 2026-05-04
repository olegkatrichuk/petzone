using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using PetZone.Core;

namespace PetZone.Framework.Cache;

public class CacheService(IMemoryCache cache) : ICacheService
{
    private static readonly ConcurrentDictionary<string, byte> Keys = new();

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        DistributedCacheEntryOptions options,
        Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        if (value is not null)
            await SetAsync(key, value, options, cancellationToken);

        return value;
    }

    public Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = options.AbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration
        };

        entryOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string s)
                Keys.TryRemove(s, out _);
        });

        cache.Set(key, value, entryOptions);
        Keys[key] = 0;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
        Keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        foreach (var key in Keys.Keys)
        {
            if (key.StartsWith(prefixKey, StringComparison.Ordinal))
            {
                cache.Remove(key);
                Keys.TryRemove(key, out _);
            }
        }
        return Task.CompletedTask;
    }
}