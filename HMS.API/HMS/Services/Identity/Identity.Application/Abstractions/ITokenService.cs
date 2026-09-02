namespace Identity.Application.Abstractions;

/// <summary>
/// Abstraksi hashing password (diimplementasikan di Infrastructure).
/// </summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);

    bool Verify(string password, string hash, string salt);
}

/// <summary>
/// Abstraksi pembuatan & validasi JWT plus refresh token (diimplementasikan di Infrastructure).
/// </summary>
public interface ITokenService
{
    TokenResult GenerateAccessToken(TokenUser user, IReadOnlyList<string> roles);

    string GenerateRefreshToken();
}

public sealed class TokenUser
{
    public Guid Id { get; init; }
    public string Username { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string? Email { get; init; }
}

public sealed class TokenResult
{
    public string AccessToken { get; init; } = default!;
    public int ExpiresInSeconds { get; init; }
}