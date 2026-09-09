using HMS.Shared.Abstractions.Domain;

namespace Identity.Domain.Entities;

/// <summary>
/// Refresh token yang disimpan di DB (bukan stateless) agar bisa di-revoke/rotate,
/// sesuai SDD Identity Service.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresOn { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset? RevokedOn { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }
    public string? ClientId { get; private set; }

    public User? User { get; private set; }

    private RefreshToken()
    { /* EF */ }

    public RefreshToken(Guid userId, string token, DateTimeOffset expiresOn, string? clientId = null, string? createdByIp = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresOn = expiresOn;
        CreatedOn = DateTimeOffset.UtcNow;
        ClientId = clientId;
        CreatedByIp = createdByIp;
    }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOn;
    public bool IsRevoked => RevokedOn is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? reason, string? revokedByIp = null, string? replacedByToken = null)
    {
        RevokedOn = DateTimeOffset.UtcNow;
        ReasonRevoked = reason;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}