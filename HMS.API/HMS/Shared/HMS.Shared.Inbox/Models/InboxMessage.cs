namespace HMS.Shared.Inbox;

/// <summary>Status pemrosesan pesan di Inbox.</summary>
public enum InboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}

/// <summary>
/// Model InboxMessage untuk menerapkan Transactional Inbox Pattern (idempotency)
/// di sisi konsumen. Selaras tabel <c>InboxEvents</c> pada SDD 04-Database-Design.
/// </summary>
public class InboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public InboxMessageStatus Status { get; set; } = InboxMessageStatus.Pending;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
}