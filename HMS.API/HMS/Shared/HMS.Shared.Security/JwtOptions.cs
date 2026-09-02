namespace HMS.Shared.Security;

/// <summary>
/// Opsi konfigurasi JWT. Dibaca dari section "Jwt" di appsettings.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "HMS.Identity";
    public string Audience { get; set; } = "hms-api";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>Waktu toleransi clock skew (default 1 menit).</summary>
    public int ClockSkewMinutes { get; set; } = 1;
}