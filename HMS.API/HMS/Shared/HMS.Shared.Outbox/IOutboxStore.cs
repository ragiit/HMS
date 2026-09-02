namespace HMS.Shared.Outbox;

/// <summary>
/// Kontrak penyimpanan pesan outbox. Diimplementasikan oleh service
/// menggunakan DbContext masing-masing (per outbox table).
/// </summary>
public interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(Guid id, string error, CancellationToken cancellationToken = default);

    Task MarkProcessingAsync(Guid id, CancellationToken cancellationToken = default);
}