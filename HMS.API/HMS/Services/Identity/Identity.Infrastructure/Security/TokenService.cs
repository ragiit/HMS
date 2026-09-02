using HMS.Identity.Application.Abstractions;
using HMS.Shared.Security;
using System.Security.Cryptography;

namespace HMS.Identity.Infrastructure.Security;

/// <summary>
/// Implementasi ITokenService yang membungkus JwtTokenService (Shared.Security)
/// dan menghasilkan refresh token acak berbasis kriptografi.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly IJwtTokenService _jwtTokenService;

    public TokenService(IJwtTokenService jwtTokenService) => _jwtTokenService = jwtTokenService;

    public TokenResult GenerateAccessToken(TokenUser user, IReadOnlyList<string> roles)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id.ToString(),
            user.Username,
            roles);

        return new TokenResult
        {
            AccessToken = accessToken,
            ExpiresInSeconds = 3600 // 60 menit, selaras JwtOptions
        };
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}