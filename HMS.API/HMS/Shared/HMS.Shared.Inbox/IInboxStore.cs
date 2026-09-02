namespace HMS.Shared.Inbox;

/// <summary>
/// Abstraction penyimpanan Inbox (idempotency). Implementasi EF Core dilakukan di
/// masing-masing service Infrastructure, namun kontrak & helper hidup di Shared supaya
/// konsisten antar service (selaras SDD 04 - tabel InboxEvents).
/// </summary>
public interface IInboxStore
{
    Task<bool> IsEventProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(InboxMessage message, CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(InboxMessage message, string error, CancellationToken cancellationToken = default);

    Task<InboxMessage?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task AddAsync(InboxMessage message, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Helper aplikasi untuk memproses pesan dengan idempotency. Memisahkan logika
/// "sudah pernah diproses?" dari logika bisnis agar setiap consumer konsisten.
/// </summary>
public interface IInboxService
{
    /// <summary>
    /// Menjamin sebuah event hanya diproses sekali. Jika event sudah diproses
    /// (berdasarkan EventId), handler TIDAK dipanggil (idempotent).
    /// </summary>
    ValueTask<bool> TryProcessAsync<TEvent>(
        TEvent @event,
        Func<TEvent, CancellationToken, Task> processHandler,
        CancellationToken cancellationToken = default) where TEvent : class;
}

/// <summary>
/// Implementasi default InboxService yang bergantung pada IInboxStore.
/// </summary>
public sealed class InboxService : IInboxService
{
    private readonly IInboxStore _store;

    public InboxService(IInboxStore store)
    {
        _store = store;
    }

    public async ValueTask<bool> TryProcessAsync<TEvent>(
        TEvent @event,
        Func<TEvent, CancellationToken, Task> processHandler,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        var eventId = ResolveEventId(@event);

        // Idempotency check
        var existing = await _store.GetByEventIdAsync(eventId, cancellationToken);
        if (existing?.Status == InboxMessageStatus.Processed)
            return false;

        var inbox = existing ?? new InboxMessage
        {
            EventId = eventId,
            EventType = @event.GetType().Name,
            RoutingKey = @event.GetType().Name,
            SourceService = ResolveSourceService(@event),
            Payload = System.Text.Json.JsonSerializer.Serialize(@event, JsonOptions)
        };

        try
        {
            await processHandler(@event, cancellationToken);
            inbox.Status = InboxMessageStatus.Processed;
            inbox.ProcessedAt = DateTimeOffset.UtcNow;
            inbox.LastAttemptAt = DateTimeOffset.UtcNow;
            inbox.LastError = null;

            if (existing is null)
                await _store.AddAsync(inbox, cancellationToken);
            await _store.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            inbox.Status = InboxMessageStatus.Failed;
            inbox.RetryCount++;
            inbox.LastAttemptAt = DateTimeOffset.UtcNow;
            inbox.LastError = ex.Message;

            if (existing is null)
                await _store.AddAsync(inbox, cancellationToken);
            else
                await _store.MarkAsFailedAsync(existing, ex.Message, cancellationToken);
            await _store.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private static Guid ResolveEventId(object @event)
    {
        // IntegrationEvent memiliki EventId
        if (@event is HMS.Shared.Contracts.IntegrationEvent integrationEvent)
            return integrationEvent.EventId;

        // Fallback: use stable key derived from type+correlation — tidak ideal, namun
        // jika consumer memakai class sendiri mereka WAJIB membawa EventId yang sama.
        var typeId = @event.GetType().FullName ?? @event.GetType().Name;
        var hashCode = @event.GetHashCode();
        return GuidUtility.CreateFromHash(typeId, hashCode);
    }

    private static string ResolveSourceService(object @event)
    {
        // Nama namespace kedua -> service (HMS.Shared.Contracts.Appointment -> Appointment)
        var @namespace = @event.GetType().Namespace;
        if (string.IsNullOrWhiteSpace(@namespace))
            return "Unknown";
        var parts = @namespace.Split('.');
        return parts.Length >= 1 ? parts[^1] : @namespace;
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
}

internal static class GuidUtility
{
    // V5-ish stable GUID dari string untuk fallback idempotency.
    public static Guid CreateFromHash(string ns, int discriminator)
    {
        using var ms = new MemoryStream();
        using (var sw = new System.IO.StreamWriter(ms))
        {
            sw.Write(ns);
            sw.Write(':');
            sw.Write(discriminator);
        }
        var bytes = System.Security.Cryptography.SHA256.HashData(ms.ToArray());
        Array.Resize(ref bytes, 16);
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50); // version 5
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // RFC 4122 variant
        return new Guid(bytes);
    }
}