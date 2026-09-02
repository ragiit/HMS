using HMS.Shared.Abstractions;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Queries;

/// <summary>Get user by ID.</summary>
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;

/// <summary>Get user by username atau email (untuk login validasi).</summary>
public sealed record GetUserByLoginQuery(string Login, string Password) : IRequest<AuthResultDto?>;

/// <summary>List user ter-paginasi.</summary>
public sealed record GetAllUsersQuery(int Page = 1, int PageSize = 10, string? Search = null)
    : IRequest<PagedResult<UserDto>>;

/// <summary>List semua role.</summary>
public sealed record GetAllRolesQuery : IRequest<IReadOnlyList<RoleDto>>;