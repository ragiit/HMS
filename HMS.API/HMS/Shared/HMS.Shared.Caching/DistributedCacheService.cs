using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HMS.Shared.Caching;

/// <summary>
/// Implementasi ICacheService berbasis IDistributedCache (Redis).
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DistributedCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        if (bytes is null) return default;

        var json = System.Text.Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue) options.AbsoluteExpirationRelativeToNow = expiration;

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(key, cancellationToken);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_cache.Get(key) is not null);
}