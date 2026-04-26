using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TL.Exemplo.Application.Contracts.Cache;

namespace TL.Exemplo.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(key, ct);
        if (json is null)
        {
            _logger.LogInformation("🔍 Cache MISS: {Key}", key);
            return default;
        }

        _logger.LogInformation("✅ Cache HIT: {Key}", key);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options, ct);
        _logger.LogInformation("💾 Cache SET: {Key} (expira em {Expiration})", key, options.AbsoluteExpirationRelativeToNow);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
        _logger.LogInformation("🗑️ Cache REMOVE: {Key}", key);
    }
}