namespace HMS.Shared.Contracts.Pharmacy;

/// <summary>Diterbitkan saat resep dibuat.</summary>
[EventName(EventNames.PrescriptionCreated)]
public sealed record PrescriptionCreatedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public IReadOnlyList<string> DrugCodes { get; init; } = Array.Empty<string>();
}

/// <summary>Diterbitkan saat resep di-dispense.</summary>
[EventName(EventNames.PrescriptionDispensed)]
public sealed record PrescriptionDispensedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; init; }
    public Guid PatientId { get; init; }
    public DateTime DispensedAt { get; init; }
    public decimal TotalAmount { get; init; }
}

/// <summary>Diterbitkan saat resep dibatalkan.</summary>
[EventName(EventNames.PrescriptionCancelled)]
public sealed record PrescriptionCancelledEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat stok barang di bawah ambang.</summary>
[EventName(EventNames.StockLow)]
public sealed record StockLowEvent : IntegrationEvent
{
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public int Remaining { get; init; }
    public int MinimumThreshold { get; init; }
}