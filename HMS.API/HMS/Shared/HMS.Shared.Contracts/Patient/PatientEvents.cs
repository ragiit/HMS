namespace HMS.Shared.Contracts.Patient;

/// <summary>Diterbitkan saat pasien baru diregistrasi.</summary>
[EventName(EventNames.PatientCreated)]
public sealed record PatientCreatedEvent : IntegrationEvent
{
    public Guid PatientId { get; init; }
    public string MedicalRecordNumber { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

/// <summary>Diterbitkan saat data pasien diperbarui.</summary>
[EventName(EventNames.PatientUpdated)]
public sealed record PatientUpdatedEvent : IntegrationEvent
{
    public Guid PatientId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

/// <summary>Diterbitkan saat pasien dinonaktifkan.</summary>
[EventName(EventNames.PatientDeactivated)]
public sealed record PatientDeactivatedEvent : IntegrationEvent
{
    public Guid PatientId { get; init; }
    public string Reason { get; init; } = string.Empty;
}