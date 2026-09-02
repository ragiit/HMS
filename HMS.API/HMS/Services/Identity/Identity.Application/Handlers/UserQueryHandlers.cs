using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Application.Queries;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Handlers;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, ct);
        return user is null ? null : MapUser(user);
    }

    internal static UserDto MapUser(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        Roles = user.Roles.Where(r => r.Role is not null).Select(r => r.Role!.Name).ToList()
    };
}

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IGenericRepository<User> _users;

    public GetAllUsersQueryHandler(IGenericRepository<User> users) => _users = users;

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var result = await _users.GetPagedAsync(
            request.Page,
            request.PageSize,
            u => string.IsNullOrWhiteSpace(request.Search) ||
                 u.Username.Contains(request.Search) ||
                 u.FullName.Contains(request.Search) ||
                 u.Email.Contains(request.Search),
            ct);

        return new PagedResult<UserDto>
        {
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                Roles = u.Roles.Where(r => r.Role is not null).Select(r => r.Role!.Name).ToList()
            }).ToList()
        };
    }
}

public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IGenericRepository<Role> _roles;

    public GetAllRolesQueryHandler(IGenericRepository<Role> roles) => _roles = roles;

    public async Task<IReadOnlyList<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken ct)
    {
        var roles = await _roles.ListAsync(r => r.IsActive, ct);
        return roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name, Description = r.Description }).ToList();
    }
}