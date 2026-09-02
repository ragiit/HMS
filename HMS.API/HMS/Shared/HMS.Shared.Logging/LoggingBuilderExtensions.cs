using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace HMS.Shared.Logging;

/// <summary>
/// Helper untuk mengonfigurasi Serilog secara konsisten antar service.
/// Output JSON terstruktur (console) + optional file + enricher.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Mengonfigurasi logger Serilog dengan sink console (JSON) + optional file.
    /// Dipanggil pada awal Program.cs setiap service sebelum builder dibangun.
    /// </summary>
    /// <param name="configuration">Konfigurasi aplikasi (membaca section "Serilog" opsional).</param>
    /// <param name="serviceName">Nama service untuk field <c>service</c> pada log.</param>
    public static void AddHmsLogging(this IConfiguration configuration, string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service", serviceName)
            .WriteTo.Console(
                outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] service={service} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/hms-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] service={service} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Mengganti logger default .NET host dengan Serilog (UseSerilog).
    /// </summary>
    public static IHostBuilder UseHmsSerilog(this IHostBuilder hostBuilder, string serviceName)
    {
        return hostBuilder.UseSerilog((context, logger) =>
        {
            logger
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service", serviceName)
                .WriteTo.Console()
                .WriteTo.File("logs/hms-.log", rollingInterval: RollingInterval.Day);
        });
    }

    /// <summary>
    /// Mendaftarkan enricher Serilog berbasis DI (correlation/user) ke logging.
    /// </summary>
    public static IServiceCollection AddHmsLoggingEnrichers(this IServiceCollection services)
    {
        services.AddSingleton<Serilog.Core.ILogEventEnricher, HttpContextLogEnricher>();
        return services;
    }
}