namespace HMS.Identity.Application.DTOs;

/// <summary>Hasil login/refresh: access token + refresh token + info user.</summary>
public sealed class AuthResultDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserDto User { get; set; } = default!;
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
}

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}