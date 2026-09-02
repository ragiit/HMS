using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace HMS.Shared.Resilience;

/// <summary>
/// Opsi konfigurasi resilience untuk HTTP self-service call (SDD 07-Cross-Cutting).
/// Menyimpan nilai retry, circuit breaker, timeout.
/// </summary>
public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public int RetryCount { get; set; } = 3;
    public double RetryBaseDelaySeconds { get; set; } = 2;   // exponential: 2,4,8 ...
    public int CircuitBreakerFailureCount { get; set; } = 5;
    public double CircuitBreakerCooldownSeconds { get; set; } = 30;
    public double TotalRequestTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Helper untuk menambahkan resilience pipeline dan typed HTTP client ke DI.
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan resilience pipeline standar (retry, circuit breaker, timeout).
    /// Semua HTTP call antar service memakainya secara konsisten.
    /// </summary>
    public static IServiceCollection AddHmsResilience(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();
        services.AddSingleton(options);

        // Pipeline default untuk HTTP calls
        services.AddResiliencePipeline("hms-http", pipeline =>
        {
            pipeline.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.RetryCount,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(options.RetryBaseDelaySeconds),
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
            });

            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                MinimumThroughput = options.CircuitBreakerFailureCount,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerCooldownSeconds)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(options.TotalRequestTimeoutSeconds));
        });

        return services;
    }

    /// <summary>
    /// Mendaftarkan resilient HTTP client untuk layanan tertentu.
    /// </summary>
    /// <typeparam name="TClient">Typed client interface.</typeparam>
    /// <typeparam name="TImplementation">Typed client implementation.</typeparam>
    public static IServiceCollection AddHmsResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string baseUrl,
        string serviceName)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("X-Service-Caller", serviceName);
        })
        .AddStandardResilienceHandler(options =>
        {
            // Standard handler berbasis Polly v8: retry + circuit breaker + timeout
        });

        return services;
    }
}