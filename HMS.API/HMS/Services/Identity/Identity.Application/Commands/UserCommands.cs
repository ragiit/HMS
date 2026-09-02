using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands;

/// <summary>Login user, menghasilkan JWT + refresh token.</summary>
public sealed record LoginCommand(string Username, string Password, string? ClientId = null) : IRequest<AuthResultDto>;

/// <summary>Registrasi user baru oleh admin, langsung dengan password.</summary>
public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string FullName,
    string Password,
    string? PhoneNumber,
    IEnumerable<string>? Roles = null) : IRequest<Guid>;

/// <summary>Refresh access token menggunakan refresh token yang valid.</summary>
public sealed record RefreshTokenCommand(string RefreshToken, string? ClientId = null) : IRequest<AuthResultDto>;

/// <summary>Revoke refresh token (logout).</summary>
public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Unit>;

/// <summary>Ubah password user yang sedang login.</summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Unit>;

/// <summary>Assign roles ke user.</summary>
public sealed record AssignRolesCommand(Guid UserId, IEnumerable<string> Roles) : IRequest<Unit>;

/// <summary>Soft-delete user.</summary>
public sealed record DeactivateUserCommand(Guid UserId) : IRequest<Unit>;

/// <summary>Update profil user (umum, non-sensitif).</summary>
public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string Email,
    string FullName,
    string? PhoneNumber) : IRequest<Unit>;