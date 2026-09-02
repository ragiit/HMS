using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Handlers;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IGenericRepository<User> _users;
    private readonly IGenericRepository<Role> _roles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IGenericRepository<User> users,
        IGenericRepository<Role> roles,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var exists = await _users.FirstOrDefaultAsync(
            u => u.Username == request.Username.Trim() || u.Email == request.Email.Trim(),
            ct);

        if (exists is not null)
            throw new ConflictException("Username atau email sudah terdaftar.");

        var user = new User(request.Username, request.Email, request.FullName, request.PhoneNumber);
        var (hash, salt) = _passwordHasher.Hash(request.Password);
        user.SetPassword(hash, salt);

        if (request.Roles is not null)
        {
            foreach (var roleName in request.Roles)
            {
                var role = await _roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
                if (role is not null)
                    user.AssignRole(role);
            }
        }

        await _users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return user.Id;
    }
}