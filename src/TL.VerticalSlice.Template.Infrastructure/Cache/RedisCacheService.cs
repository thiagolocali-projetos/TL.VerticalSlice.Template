using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TL.VerticalSlice.Template.Application.Contracts.Cache;

namespace TL.VerticalSlice.Template.Infrastructure.Cache;

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
            _logger.LogInformation("ðŸ” Cache MISS: {Key}", key);
            return default;
        }

        _logger.LogInformation("âœ… Cache HIT: {Key}", key);
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
        _logger.LogInformation("ðŸ’¾ Cache SET: {Key} (expira em {Expiration})", key, options.AbsoluteExpirationRelativeToNow);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
        _logger.LogInformation("ðŸ—‘ï¸ Cache REMOVE: {Key}", key);
    }
}
