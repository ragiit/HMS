using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Handlers;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IGenericRepository<RefreshToken> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(
        IGenericRepository<RefreshToken> refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        var stored = await _refreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (stored is not null)
        {
            stored.Revoke("logout");
            _refreshTokens.Update(stored);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        return Unit.Value;
    }
}

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IExecutionContext _executionContext;

    public ChangePasswordCommandHandler(
        IGenericRepository<User> users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IExecutionContext executionContext)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw new BusinessRuleViolationException("Password saat ini salah.");

        var (newHash, newSalt) = _passwordHasher.Hash(request.NewPassword);
        user.SetPassword(newHash, newSalt);
        user.ResetFailedLogin();
        user.UpdateAudit(_executionContext.UserName);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

public sealed class AssignRolesCommandHandler : IRequestHandler<AssignRolesCommand, Unit>
{
    private readonly IGenericRepository<User> _users;
    private readonly IGenericRepository<Role> _roles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public AssignRolesCommandHandler(
        IGenericRepository<User> users,
        IGenericRepository<Role> roles,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(AssignRolesCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Reset roles user dahulu
        foreach (var existing in user.Roles.ToList())
            user.RemoveRole(existing.RoleId);

        foreach (var roleName in request.Roles.Distinct())
        {
            var role = await _roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
            if (role is not null && role.IsActive)
                user.AssignRole(role);
        }

        user.UpdateAudit(_executionContext.UserName);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public DeactivateUserCommandHandler(
        IGenericRepository<User> users,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        user.MarkDeleted();
        user.UpdateAudit(_executionContext.UserName);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Unit>
{
    private readonly IGenericRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public UpdateUserProfileCommandHandler(
        IGenericRepository<User> users,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        user.UpdateProfile(request.Email, request.FullName, request.PhoneNumber);
        user.UpdateAudit(_executionContext.UserName);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}