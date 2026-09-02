namespace HMS.Shared.Contracts.Appointment;
/// <summary>Diterbitkan saat janji temu berhasil di-booking.</summary>
[EventName(EventNames.AppointmentBooked)]
public sealed record AppointmentBookedEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int QueueNumber { get; init; }
}

/// <summary>Diterbitkan saat janji temu dibatalkan.</summary>
[EventName(EventNames.AppointmentCancelled)]
public sealed record AppointmentCancelledEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat janji temu selesai.</summary>
[EventName(EventNames.AppointmentCompleted)]
public sealed record AppointmentCompletedEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime CompletedAt { get; init; }
}

/// <summary>Diterbitkan saat janji temu di-reschedule.</summary>
[EventName(EventNames.AppointmentRescheduled)]
public sealed record AppointmentRescheduledEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime OldScheduledAt { get; init; }
    public DateTime NewScheduledAt { get; init; }
}

/// <summary>Diterbitkan saat pasien check-in (datang).</summary>
[EventName(EventNames.AppointmentCheckedIn)]
public sealed record AppointmentCheckedInEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public DateTime CheckedInAt { get; init; }
}

/// <summary>Diterbitkan saat pasien tidak hadir (no-show).</summary>
[EventName(EventNames.AppointmentNoShow)]
public sealed record AppointmentNoShowEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public string? Reason { get; init; }
}

/// <summary>Diterbitkan saat pengingat janji temu jatuh tempo.</summary>
[EventName(EventNames.AppointmentReminder)]
public sealed record AppointmentReminderEvent : IntegrationEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledAt { get; init; }
}