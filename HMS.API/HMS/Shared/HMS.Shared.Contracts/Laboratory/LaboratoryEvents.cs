namespace HMS.Shared.Contracts.Laboratory;

/// <summary>Diterbitkan saat hasil lab siap (dikonsumsi MedicalRecord & Notification).</summary>
[EventName(EventNames.LabResultReady)]
public sealed record LabResultReadyEvent : IntegrationEvent
{
    public Guid LabOrderId { get; init; }
    public Guid PatientId { get; init; }
    public Guid MedicalRecordId { get; init; }
    public bool HasCriticalResult { get; init; }
}

/// <summary>Diterbitkan saat ada hasil lab diluar range kritis (urgent).</summary>
[EventName(EventNames.LabResultCritical)]
public sealed record LabResultCriticalEvent : IntegrationEvent
{
    public Guid LabOrderId { get; init; }
    public Guid PatientId { get; init; }
    public string ParameterCode { get; init; } = string.Empty;
    public string ParameterName { get; init; } = string.Empty;
    public string ResultValue { get; init; } = string.Empty;
    public string? Unit { get; init; }
}

/// <summary>Diterbitkan saat order lab dibatalkan.</summary>
[EventName(EventNames.LabOrderCancelled)]
public sealed record LabOrderCancelledEvent : IntegrationEvent
{
    public Guid LabOrderId { get; init; }
    public Guid PatientId { get; init; }
    public string Reason { get; init; } = string.Empty;
}