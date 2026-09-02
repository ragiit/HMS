namespace HMS.Shared.Caching;

/// <summary>
/// Abstraksi untuk operasi cache dengan serialization JSON otomatis.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Menyediakan formatting key cache standar agar konsisten antar service.
/// </summary>
public static class CacheKeys
{
    public static string For(string prefix, object id) => $"{prefix}:{id}";

    public static string ForList(string prefix, object id) => $"{prefix}:list:{id}";
}