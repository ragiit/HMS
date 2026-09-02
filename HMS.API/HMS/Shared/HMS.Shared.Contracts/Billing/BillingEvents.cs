namespace HMS.Shared.Contracts.Billing;

/// <summary>Diterbitkan saat invoice diterbitkan.</summary>
[EventName(EventNames.BillIssued)]
public sealed record BillIssuedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; init; }
    public Guid PatientId { get; init; }
    public decimal TotalAmount { get; init; }
    public string ReferenceType { get; init; } = string.Empty; // Appointment/Lab/Pharmacy/Treatment
    public string Currency { get; init; } = "IDR";
}

/// <summary>Diterbitkan saat pembayaran diterima.</summary>
[EventName(EventNames.PaymentReceived)]
public sealed record PaymentReceivedEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat tagihan melewati due date.</summary>
[EventName(EventNames.PaymentOverdue)]
public sealed record PaymentOverdueEvent : IntegrationEvent
{
    public Guid InvoiceId { get; init; }
    public Guid PatientId { get; init; }
    public decimal AmountDue { get; init; }
    public DateOnly DueDate { get; init; }
}

/// <summary>Diterbitkan saat invoice dibatalkan/void.</summary>
[EventName(EventNames.InvoiceCancelled)]
public sealed record InvoiceCancelledEvent : IntegrationEvent
{
    public Guid InvoiceId { get; init; }
    public Guid PatientId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Diterbitkan saat refund diproses.</summary>
[EventName(EventNames.RefundProcessed)]
public sealed record RefundProcessedEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}