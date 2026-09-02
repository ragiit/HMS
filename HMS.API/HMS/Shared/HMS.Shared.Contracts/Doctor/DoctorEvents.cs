namespace HMS.Shared.Contracts.Doctor;

/// <summary>Diterbitkan saat dokter baru didaftarkan.</summary>
[EventName(EventNames.DoctorCreated)]
public sealed record DoctorCreatedEvent : IntegrationEvent
{
    public Guid DoctorId { get; init; }
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string EmployeeNumber { get; init; } = string.Empty;
    public string? Specialization { get; init; }
    public string? Email { get; init; }
}

/// <summary>Diterbitkan saat profil dokter diperbarui.</summary>
[EventName(EventNames.DoctorUpdated)]
public sealed record DoctorUpdatedEvent : IntegrationEvent
{
    public Guid DoctorId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

/// <summary>Diterbitkan saat jadwal praktek dokter berubah (Appointment harus re-check availability).</summary>
[EventName(EventNames.DoctorScheduleChanged)]
public sealed record DoctorScheduleChangedEvent : IntegrationEvent
{
    public Guid DoctorId { get; init; }
    public Guid? ScheduleId { get; init; }
    public string? Reason { get; init; }
}

/// <summary>Diterbitkan saat cuti/off (leave) ditambahkan ke jadwal dokter.</summary>
[EventName(EventNames.DoctorLeaveAdded)]
public sealed record DoctorLeaveAddedEvent : IntegrationEvent
{
    public Guid DoctorId { get; init; }
    public DateOnly ExceptionDate { get; init; }
    public bool IsFullDayCancelled { get; init; }
    public string? Reason { get; init; }
}