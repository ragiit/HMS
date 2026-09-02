using HMS.Shared.Abstractions.Domain;

namespace HMS.Identity.Domain.Entities;

/// <summary>
/// Role pengguna (Admin, Doctor, Nurse, FrontDesk, Pharmacist, LabStaff,
/// BillingStaff, Patient, Inventory) sesuai SDD 04 Identity Service.
/// </summary>
public sealed class Role : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedDate { get; private set; }

    private Role()
    { /* EF */ }

    public Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        CreatedDate = DateTimeOffset.UtcNow;
    }

    public void Update(string? description)
    {
        Description = description;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}