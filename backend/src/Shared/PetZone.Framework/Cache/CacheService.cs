using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PetZone.Core;
using StackExchange.Redis;

namespace PetZone.Framework.Cache;

public class CacheService(
    IDistributedCache cache,
    IConnectionMultiplexer multiplexer) : ICacheService
{
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        DistributedCacheEntryOptions options,
        Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await factory();
        if (value is not null)
            await SetAsync(key, value, options, cancellationToken);

        return value;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var data = await cache.GetStringAsync(key, cancellationToken);
        return data is null ? null : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var data = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, data, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(key, cancellationToken);

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var db = multiplexer.GetDatabase();
        var server = multiplexer.GetServer(multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();
        if (keys.Length > 0)
            await db.KeyDeleteAsync(keys);
    }
}