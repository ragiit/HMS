using HMS.Shared.Abstractions.Domain;
using HMS.Shared.Abstractions.Persistence;
using Identity.Domain.Events;

namespace Identity.Domain.Entities;

/// <summary>
/// Aggregate Root utk user. Menyimpan kredensial, status aktivasi/lockout, dan relasi role.
/// Merupakan source of truth autentikasi di seluruh sistem.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IAuditableEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsLockedOut { get; private set; }
    public DateTimeOffset? LockoutEndDate { get; private set; }
    public DateTimeOffset? LastLoginDate { get; private set; }
    public int AccessFailedCount { get; private set; }

    // Audit & soft delete
    public DateTimeOffset CreatedDate { get; private set; }

    public string? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTimeOffset? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    public ICollection<UserRole> Roles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User()
    { /* EF */ }

    public User(string username, string email, string fullName, string? phoneNumber)
    {
        Id = Guid.NewGuid();
        Username = username.Trim();
        Email = email.Trim();
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        CreatedDate = DateTimeOffset.UtcNow;
    }

    public void SetPassword(string passwordHash, string passwordSalt)
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public void AssignRole(Role role)
    {
        if (Roles.All(r => r.RoleId != role.Id))
            Roles.Add(new UserRole(this.Id, role.Id));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = Roles.FirstOrDefault(r => r.RoleId == roleId);
        if (existing is not null)
            Roles.Remove(existing);
    }

    public void UpdateProfile(string email, string fullName, string? phoneNumber)
    {
        Email = email.Trim();
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        ModifiedDate = DateTimeOffset.UtcNow;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void RecordLogin() => LastLoginDate = DateTimeOffset.UtcNow;

    public void RecordFailedLogin(int maxAttempts = 5)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxAttempts)
        {
            IsLockedOut = true;
            LockoutEndDate = DateTimeOffset.UtcNow.AddMinutes(15);
        }
    }

    public void ResetFailedLogin()
    {
        AccessFailedCount = 0;
        IsLockedOut = false;
        LockoutEndDate = null;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedDate = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserDeactivatedDomainEvent(Id, Username));
    }

    public void UpdateAudit(string? modifiedBy)
    {
        ModifiedDate = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }

    public object? GetId() => Id;
}