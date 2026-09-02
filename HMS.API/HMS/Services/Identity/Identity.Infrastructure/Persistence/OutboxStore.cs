using HMS.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace HMS.Identity.Infrastructure.Persistence;

/// <summary>
/// Implementasi IOutboxStore berbasis IdentityDbContext (tabel OutboxEvents).
/// </summary>
public sealed class OutboxStore : IOutboxStore
{
    private readonly IdentityDbContext _dbContext;

    public OutboxStore(IdentityDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize, CancellationToken cancellationToken = default)
        => await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
        => UpdateAsync(id, m => { m.Status = OutboxStatus.Processed; m.ProcessedOn = DateTime.UtcNow; });

    public Task MarkFailedAsync(Guid id, string error, CancellationToken cancellationToken = default)
        => UpdateAsync(id, m => { m.Status = OutboxStatus.Failed; m.LastError = error; });

    public Task MarkProcessingAsync(Guid id, CancellationToken cancellationToken = default)
        => UpdateAsync(id, m => m.Status = OutboxStatus.Processing);

    private async Task UpdateAsync(Guid id, Action<OutboxMessage> update)
    {
        var msg = await _dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (msg is not null)
        {
            update(msg);
            await _dbContext.SaveChangesAsync();
        }
    }
}