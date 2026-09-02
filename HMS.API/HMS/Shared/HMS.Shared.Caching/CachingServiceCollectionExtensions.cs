using HMS.Shared.Caching;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods untuk mendaftarkan layanan caching ke DI.
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan Redis (IDistributedCache) + ICacheService.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="redisConnectionString">Connection string Redis. Contoh: "localhost:6379".</param>
    public static IServiceCollection AddHmsCaching(this IServiceCollection services, string redisConnectionString)
    {
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        services.AddScoped<ICacheService, DistributedCacheService>();
        return services;
    }
}