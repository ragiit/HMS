using HMS.Shared.Contracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace HMS.Shared.Messaging;

/// <summary>
/// Implementasi IMessagePublisher berbasis RabbitMQ.
/// </summary>
public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqPublisher(IConnection connection, RabbitMqOptions options, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _options = options;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        await Task.Yield(); // tetap async-friendly

        var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: _options.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            Type = @event.EventType,
            MessageId = @event.EventId.ToString(),
            CorrelationId = @event.CorrelationId.ToString(),
            Headers = new Dictionary<string, object?>
            {
                ["event_type"] = @event.EventType
            }
        };

        var key = string.IsNullOrWhiteSpace(routingKey) ? @event.RoutingKey : routingKey;
        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: key,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published event {EventType} to {Exchange} with key {RoutingKey}",
            @event.EventType, _options.ExchangeName, key);

        await channel.CloseAsync(cancellationToken: cancellationToken);
    }
}