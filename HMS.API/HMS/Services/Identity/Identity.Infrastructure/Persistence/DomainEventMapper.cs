using HMS.Shared.Abstractions.Domain;
using HMS.Shared.Contracts.Identity;
using Identity.Domain.Events;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Implementasi pemetaan domain event → integration event untuk Identity Service.
/// </summary>
public sealed class DomainEventMapper : IDomainEventMapper
{
    public object? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        UserDeactivatedDomainEvent e => new UserDeactivatedEvent
        {
            UserId = e.UserId,
            Reason = "deactivated"
        },
        _ => null
    };
}