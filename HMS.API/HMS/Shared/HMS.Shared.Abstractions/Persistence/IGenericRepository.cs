using System.Linq.Expressions;

namespace HMS.Shared.Abstractions.Persistence;

/// <summary>
/// Generic repository untuk operasi CRUD dasar pada aggregate.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task<PagedResult<T>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);
}