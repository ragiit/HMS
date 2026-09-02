namespace HMS.Shared.Outbox;

/// <summary>
/// Abstraksi untuk menerbitkan pesan outbox ke message bus.
/// Implementasinya memakai RabbitMQ (lihat HMS.Shared.Messaging).
/// </summary>
public interface IOutboxPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}