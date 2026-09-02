namespace HMS.Shared.Contracts.Identity;

/// <summary>Diterbitkan saat user baru dibuat (misal setelah registrasi pasien/staff).</summary>
[EventName(EventNames.UserCreated)]
public sealed record UserCreatedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? FullName { get; init; }
}

/// <summary>Diterbitkan saat role user berubah.</summary>
[EventName(EventNames.UserRoleChanged)]
public sealed record UserRoleChangedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string PreviousRole { get; init; } = string.Empty;
    public string NewRole { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat profil user diperbarui.</summary>
[EventName(EventNames.UserUpdated)]
public sealed record UserUpdatedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
}

/// <summary>Diterbitkan saat user dinonaktifkan.</summary>
[EventName(EventNames.UserDeactivated)]
public sealed record UserDeactivatedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
}