namespace HMS.Shared.Abstractions.Domain;

/// <summary>
/// Base class untuk Aggregate Root DDD. Memiliki koleksi domain events
/// yang akan di-clear saat disimpan.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
}

/// <summary>
/// Marker interface untuk aggregate root.
/// </summary>
public interface IAggregateRoot
{
}