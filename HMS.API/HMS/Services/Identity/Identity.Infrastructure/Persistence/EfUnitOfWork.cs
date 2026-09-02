using HMS.Shared.Abstractions.Domain;
using HMS.Shared.Abstractions.Persistence;
using HMS.Shared.Outbox;
using System.Text.Json;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Unit of Work yang me-save changes dan menulis domain events aggregate ke tabel
/// OutboxEvents dalam SATU transaksi yang sama (Transaction Outbox Pattern).
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _dbContext;
    private readonly IDomainEventMapper _domainEventMapper;

    public EfUnitOfWork(IdentityDbContext dbContext, IDomainEventMapper domainEventMapper)
    {
        _dbContext = dbContext;
        _domainEventMapper = domainEventMapper;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CollectDomainEventsToOutbox();
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        CollectDomainEventsToOutbox();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void CollectDomainEventsToOutbox()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            var entity = entry.Entity;
            var events = GetDomainEvents(entity);
            if (events is null || events.Count == 0)
                continue;

            foreach (var domainEvent in events)
            {
                var integrationEvent = _domainEventMapper.Map(domainEvent);
                if (integrationEvent is null)
                    continue;

                _dbContext.OutboxMessages.Add(new OutboxMessage
                {
                    Id = domainEvent.EventId,
                    Type = integrationEvent.GetType().AssemblyQualifiedName!,
                    Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                    OccurredOn = DateTime.UtcNow
                });
            }

            ClearDomainEvents(entity);
        }
    }

    private static IReadOnlyCollection<IDomainEvent>? GetDomainEvents(object entity)
    {
        var prop = entity.GetType().GetProperty("DomainEvents");
        return prop?.GetValue(entity) as IReadOnlyCollection<IDomainEvent>;
    }

    private static void ClearDomainEvents(object entity)
    {
        var method = entity.GetType().GetMethod("ClearDomainEvents");
        method?.Invoke(entity, null);
    }
}