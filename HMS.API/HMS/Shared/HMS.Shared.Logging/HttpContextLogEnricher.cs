using HMS.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace HMS.Shared.Logging;

/// <summary>
/// Serilog enricher yang menambahkan <c>correlationId</c>, <c>userId</c>, dan <c>service</c>
/// ke setiap event log sehingga format log JSON konsisten (SDD 07-Cross-Cutting log structure).
/// </summary>
public sealed class HttpContextLogEnricher : ILogEventEnricher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _serviceName;

    public HttpContextLogEnricher(IServiceProvider serviceProvider, string? serviceName = null)
    {
        _serviceProvider = serviceProvider;
        _serviceName = serviceName;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            // Resolve IExecutionContext (CurrentUser) untuk correlationId & userId
            var context = scope.ServiceProvider.GetService<IExecutionContext>();
            if (context is not null)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
                    "correlationId", context.CorrelationId));
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
                    "userId", context.UserId));
            }
        }
        catch
        {
            // Jangan biarkan logging gagal merusak request
        }

        if (!string.IsNullOrEmpty(_serviceName))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("service", _serviceName));
        }
    }
}