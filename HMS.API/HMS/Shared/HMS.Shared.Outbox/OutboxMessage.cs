namespace HMS.Shared.Outbox;

/// <summary>
/// Status perjalanan sebuah pesan outbox.
/// </summary>
public enum OutboxStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}

/// <summary>
/// Entitas persisten untuk menangkap integration event sebelum dikirim ke bus.
/// Ini inti dari transactional Outbox Pattern.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;        // Nama tipe event
    public string Payload { get; set; } = string.Empty;     // JSON serialized event
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? LastError { get; set; }
}