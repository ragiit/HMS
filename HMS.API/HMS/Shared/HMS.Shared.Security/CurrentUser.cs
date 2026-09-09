using HMS.Shared.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.Shared.Security;

/// <summary>
/// Mengakses user yang sedang terautentikasi dari claims pada request saat ini.
/// Memudahkan setiap service mendapatkan userId, role, dll secara konsisten.
/// </summary>
public sealed class CurrentUser : IExecutionContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor?.HttpContext?.User;
    private HttpContext? Http => _httpContextAccessor?.HttpContext;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub")
        ?? User?.FindFirstValue("nameid");

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("unique_name");

    public string? TenantId =>
        User?.FindFirstValue("tenant_id") ?? User?.FindFirstValue("tenant");

    public string? IpAddress
    {
        get
        {
            if (Http is null)
                return null;

            var forwarded = Http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                // Ambil client IP pertama jika request melalui multi-hop proxy
                return forwarded.Split(',')[0].Trim();
            }

            return Http.Connection.RemoteIpAddress?.ToString();
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roles = User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();
            return roles ?? new List<string>();
        }
    }

    public string CorrelationId =>
        Http?.Items["CorrelationId"]?.ToString()
        ?? Http?.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? CorrelationIdGenerator.Generate();

    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasClaim(string type, string? value = null) =>
        value is null
            ? User?.HasClaim(c => c.Type == type) ?? false
            : User?.HasClaim(type, value) ?? false;
}