namespace HMS.Shared.Contracts.MedicalRecord;

/// <summary>Diterbitkan saat rekam medis baru dibuat.</summary>
[EventName(EventNames.MedicalRecordCreated)]
public sealed record MedicalRecordCreatedEvent : IntegrationEvent
{
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public Guid AppointmentId { get; init; }
    public string? DiagnosisCode { get; init; }
}

/// <summary>Diterbitkan saat rekam medis difinalisasi (ditandatangani dokter).</summary>
[EventName(EventNames.MedicalRecordFinalized)]
public sealed record MedicalRecordFinalizedEvent : IntegrationEvent
{
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public Guid AppointmentId { get; init; }
    public bool HasPrescription { get; init; }
}

/// <summary>Diterbitkan saat tindakan/prosedur ditambahkan.</summary>
[EventName(EventNames.MedicalRecordTreatmentAdded)]
public sealed record MedicalRecordTreatmentAddedEvent : IntegrationEvent
{
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public string TreatmentCode { get; init; } = string.Empty;
    public string TreatmentName { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat tanda vital diperbarui.</summary>
[EventName(EventNames.VitalSignsUpdated)]
public sealed record VitalSignsUpdatedEvent : IntegrationEvent
{
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
}

/// <summary>Diterbitkan saat order lab dibuat (dikonsumsi Laboratory Service).</summary>
[EventName(EventNames.LabOrderCreated)]
public sealed record LabOrderCreatedEvent : IntegrationEvent
{
    public Guid LabOrderId { get; init; }
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public IReadOnlyList<string> TestCodes { get; init; } = Array.Empty<string>();
}

/// <summary>Diterbitkan saat permintaan resep dibuat (dikonsumsi Pharmacy Service).</summary>
[EventName(EventNames.PrescriptionOrderCreated)]
public sealed record PrescriptionOrderCreatedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; init; }
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public IReadOnlyList<PrescriptionItemDto> Items { get; init; } = Array.Empty<PrescriptionItemDto>();
}

public sealed record PrescriptionItemDto
{
    public string DrugCode { get; init; } = string.Empty;
    public string DrugName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string Dosage { get; init; } = string.Empty;
}