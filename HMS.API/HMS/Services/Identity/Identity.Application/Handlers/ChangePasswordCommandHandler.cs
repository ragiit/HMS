using MediatR;
using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;

namespace Identity.Application.Handlers;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null || user.IsDeleted)
            throw new NotFoundException("User", request.UserId);

        if (!user.IsActive)
            throw new PermissionDeniedException("Your account is currently disabled. Please contact your administrator.");

        // Verify current password
        var isCurrentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt);
        if (!isCurrentPasswordValid)
            throw new UnauthorizedException("Current password does not match.");

        // Hash new password and generate a new salt
        var (newHash, newSalt) = _passwordHasher.Hash(request.NewPassword);
        user.SetPassword(newHash, newSalt);

        // Security best practice: Revoke all active sessions across devices
        user.RevokeAllRefreshTokens("Password changed by user", _executionContext.IpAddress);

        user.UpdateAudit(_executionContext.UserName ?? user.Username);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}