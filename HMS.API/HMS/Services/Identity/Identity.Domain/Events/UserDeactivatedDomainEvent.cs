using HMS.Shared.Abstractions.Domain;

namespace HMS.Identity.Domain.Events;

/// <summary>
/// Domain event saat user dinonaktifkan (dipublish sebagai integration event user.deactivated).
/// </summary>
public sealed record UserDeactivatedDomainEvent(Guid UserId, string Username) : DomainEvent;