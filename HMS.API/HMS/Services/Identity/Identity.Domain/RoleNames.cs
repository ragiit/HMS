namespace Identity.Domain;

/// <summary>
/// Konstanta nama role sesuai SDD Identity Service (seed data).
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string FrontDesk = "FrontDesk";
    public const string Pharmacist = "Pharmacist";
    public const string LabStaff = "LabStaff";
    public const string BillingStaff = "BillingStaff";
    public const string Patient = "Patient";
    public const string Inventory = "Inventory";

    public static IReadOnlyList<string> All => new[]
    {
        Admin, Doctor, Nurse, FrontDesk, Pharmacist, LabStaff, BillingStaff, Patient, Inventory
    };
}