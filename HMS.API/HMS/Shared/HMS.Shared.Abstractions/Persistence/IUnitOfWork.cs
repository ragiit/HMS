namespace HMS.Shared.Abstractions.Persistence;

/// <summary>
/// Unit of Work - mengelola satu transaksi DB dan memflush domain events.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}