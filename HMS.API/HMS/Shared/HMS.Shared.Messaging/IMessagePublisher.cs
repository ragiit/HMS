using HMS.Shared.Contracts;

namespace HMS.Shared.Messaging;

/// <summary>
/// Menerbitkan integration event ke message bus (RabbitMQ).
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}