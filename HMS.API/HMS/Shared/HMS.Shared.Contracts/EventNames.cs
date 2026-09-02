namespace HMS.Shared.Contracts;

/// <summary>
/// Menandai integration event sebagai published pattern naming helper.
/// Nilai di sini harus SELALU identik dengan routing key yang dipakai di SDD
/// 06-Messaging (format "{service}.{entity}.{action}").
/// </summary>
public static class EventNames
{
    // Identity
    public const string UserCreated = "identity.user.created";

    public const string UserUpdated = "identity.user.updated";
    public const string UserRoleChanged = "identity.user.role_changed";
    public const string UserDeactivated = "identity.user.deactivated";

    // Patient
    public const string PatientCreated = "patient.created";

    public const string PatientUpdated = "patient.updated";
    public const string PatientDeactivated = "patient.deactivated";

    // Doctor
    public const string DoctorCreated = "doctor.created";

    public const string DoctorUpdated = "doctor.updated";
    public const string DoctorScheduleChanged = "doctor.schedule.changed";
    public const string DoctorLeaveAdded = "doctor.leave.added";

    // Appointment
    public const string AppointmentBooked = "appointment.booked";

    public const string AppointmentRescheduled = "appointment.rescheduled";
    public const string AppointmentCancelled = "appointment.cancelled";
    public const string AppointmentCompleted = "appointment.completed";
    public const string AppointmentCheckedIn = "appointment.checked_in";
    public const string AppointmentNoShow = "appointment.no_show";
    public const string AppointmentReminder = "appointment.reminder_due";

    // Medical Record
    public const string MedicalRecordCreated = "medical_record.created";

    public const string MedicalRecordFinalized = "medical_record.finalized";
    public const string MedicalRecordTreatmentAdded = "medical_record.treatment_added";
    public const string VitalSignsUpdated = "vital_signs.updated";
    public const string LabOrderCreated = "lab.order_created";
    public const string PrescriptionOrderCreated = "prescription.order_created";

    // Laboratory
    public const string LabResultReady = "lab.result_ready";

    public const string LabResultCritical = "lab.result.critical";
    public const string LabOrderCancelled = "lab.order.cancelled";

    // Billing
    public const string BillIssued = "bill.issued";

    public const string PaymentReceived = "bill.payment_received";
    public const string PaymentOverdue = "payment.overdue";
    public const string InvoiceCancelled = "invoice.cancelled";
    public const string RefundProcessed = "refund.processed";

    // Pharmacy
    public const string PrescriptionCreated = "prescription.created";

    public const string PrescriptionDispensed = "prescription.dispensed";
    public const string PrescriptionCancelled = "prescription.cancelled";

    // Inventory
    public const string InventoryReceived = "inventory.received";

    public const string InventoryStockAdjusted = "inventory.stock.adjusted";
    public const string StockLow = "inventory.stock_low";
    public const string InventoryLowStock = "inventory.low_stock";
    public const string InventoryOutOfStock = "inventory.out_of_stock";
    public const string InventoryExpiring = "inventory.expiring";

    /// <summary>
    /// Membantu memetakan routing key ke nama kelas event jika diperlukan.
    /// </summary>
    public static string For<TEvent>() where TEvent : IntegrationEvent => EventNameResolver.Resolve<TEvent>();
}