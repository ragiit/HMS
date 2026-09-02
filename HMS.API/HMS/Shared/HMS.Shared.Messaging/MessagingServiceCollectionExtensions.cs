using HMS.Shared.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace HMS.Shared.Messaging;

/// <summary>
/// Extension methods untuk mendaftarkan layanan messaging (RabbitMQ) ke DI.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan koneksi RabbitMQ, publisher & subscriber.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Konfigurasi; membaca section "RabbitMq".</param>
    public static IServiceCollection AddHmsMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddSingleton(options);

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost
            };

            return factory.CreateConnectionAsync()
                .GetAwaiter()
                .GetResult();
        });

        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IMessageSubscriber, RabbitMqSubscriber>();

        return services;
    }

    /// <summary>
    /// Mendaftarkan {@c IOutboxPublisher} (RabbitMqOutboxPublisher) yang mengirim outbox
    /// pesan ke bus. Konsumen wajib mendaftarkan IOutboxStore sendiri (implementasi EF Core).
    /// </summary>
    public static IServiceCollection AddHmsOutboxPublisher(this IServiceCollection services)
    {
        services.AddScoped<IOutboxPublisher, RabbitMqOutboxPublisher>();
        return services;
    }

    /// <summary>
    /// Mendaftarkan OutboxProcessor (BackgroundService) beserta publisher-nya.
    /// IOutboxStore tetap wajib disediakan oleh service.
    /// </summary>
    public static IServiceCollection AddHmsOutboxProcessor(
        this IServiceCollection services,
        TimeSpan? pollingInterval = null,
        int batchSize = 50)
    {
        services.AddHostedService(sp => new OutboxProcessor(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OutboxProcessor>>(),
            pollingInterval,
            batchSize));

        services.AddHmsOutboxPublisher();
        return services;
    }
}