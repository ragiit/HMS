using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HMS.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementasi EF Core dari <see cref="IGenericRepository{T}"/>.
/// </summary>
public sealed class EfGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _set;

    public EfGenericRepository(IdentityDbContext dbContext) => _set = dbContext.Set<T>();

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        => await _set.FindAsync(new object[] { id! }, cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _set.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Delete(T entity) => _set.Remove(entity);
}