using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS.Shared.Security;

/// <summary>
/// Layanan pembuatan JWT access token & refresh token, serta validasi.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string username, IEnumerable<string> roles, IEnumerable<Claim>? extraClaims = null);

    ClaimsPrincipal? ValidateToken(string token);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
    }

    public string GenerateAccessToken(
        string userId,
        string username,
        IEnumerable<string> roles,
        IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromMinutes(_options.ClockSkewMinutes)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}