namespace HMS.Shared.Abstractions;

/// <summary>
/// Metadata untuk melacak sumber operasi (untuk retry/deduplication event).
/// </summary>
public sealed record OperationContext
{
    public Guid CorrelationId { get; init; }
    public string? UserId { get; init; }
    public string? TenantId { get; init; }
}