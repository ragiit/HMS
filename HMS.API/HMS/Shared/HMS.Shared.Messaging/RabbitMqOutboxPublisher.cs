using HMS.Shared.Contracts;
using HMS.Shared.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HMS.Shared.Messaging;

/// <summary>
/// Implementasi IOutboxPublisher yang mengirim OutboxMessage
/// ke bus via IMessagePublisher.
/// </summary>
public class RabbitMqOutboxPublisher : IOutboxPublisher
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqOutboxPublisher(IMessagePublisher publisher, ILogger<RabbitMqOutboxPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishAsync(
    OutboxMessage message,
    CancellationToken cancellationToken = default)
    {
        var eventType = Type.GetType(message.Type);

        if (eventType is null)
        {
            _logger.LogError(
                "Tidak dapat menemukan event type {Type} untuk OutboxMessage {Id}",
                message.Type,
                message.Id);

            return;
        }

        if (!typeof(IntegrationEvent).IsAssignableFrom(eventType))
        {
            _logger.LogError(
                "Type {Type} bukan merupakan IntegrationEvent untuk OutboxMessage {Id}",
                message.Type,
                message.Id);

            return;
        }

        var payload = JsonSerializer.Deserialize(
            message.Payload,
            eventType,
            JsonOptions) as IntegrationEvent;

        if (payload is null)
        {
            _logger.LogError(
                "Gagal deserialize payload untuk OutboxMessage {Id}, Type {Type}",
                message.Id,
                message.Type);

            return;
        }

        await _publisher.PublishAsync(
            payload,
            routingKey: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Outbox message {Id} ({Type}) diterbitkan",
            message.Id,
            message.Type);
    }
}