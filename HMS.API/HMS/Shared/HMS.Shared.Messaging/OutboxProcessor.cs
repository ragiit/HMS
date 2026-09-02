using HMS.Shared.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HMS.Shared.Messaging;

/// <summary>
/// Background worker yang membaca pesan outbox yang <c>Pending</c>, menerbitkannya
/// ke message bus, lalu menandainya <c>Processed</c>. Ini Jantung dari Transactional
/// Outbox Pattern (SDD 06-Messaging bagian 4).
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly int _batchSize;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger,
        TimeSpan? pollingInterval = null,
        int batchSize = 50)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _batchSize = batchSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor dimulai. Interval: {Interval}s, Batch: {Batch}",
            _pollingInterval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutdown normal
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memproses outbox pada siklus ini.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor berhenti.");
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var publisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();

        var pending = await store.GetPendingAsync(_batchSize, cancellationToken);
        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await store.MarkProcessingAsync(message.Id, cancellationToken);

            try
            {
                await publisher.PublishAsync(message, cancellationToken);
                await store.MarkProcessedAsync(message.Id, cancellationToken);
                _logger.LogInformation("Outbox {Id} ({Type}) berhasil diterbitkan.", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menerbitkan outbox {Id} ({Type}).", message.Id, message.Type);
                message.RetryCount++;
                await store.MarkFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}