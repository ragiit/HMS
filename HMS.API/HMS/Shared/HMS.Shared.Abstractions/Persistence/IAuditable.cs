namespace HMS.Shared.Abstractions.Persistence;

/// <summary>
/// Marker interface untuk soft-delete entity. Service dapat memfilter otomatis
/// pada query (global query filter) dan menyimpan timestamps.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedDate { get; }
}

/// <summary>
/// Marker untuk audit trail pada entity: melacak pembuat & pengubah.
/// Selaras dengan SDD 07-Cross-Cutting Audit Trail.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedDate { get; }
    string? CreatedBy { get; }
    DateTimeOffset? ModifiedDate { get; }
    string? ModifiedBy { get; }
}

/// <summary>
/// Menyediakan nilai RowVersion untuk optimistic concurrency (SQL Server ROWVERSION).
/// </summary>
public interface IConcurrencyEntity
{
    byte[]? RowVersion { get; }
}