using HMS.Shared.Abstractions.Domain;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Memetakan domain event menjadi integration event yang akan ditulis ke outbox.
/// </summary>
public interface IDomainEventMapper
{
    object? Map(IDomainEvent domainEvent);
}