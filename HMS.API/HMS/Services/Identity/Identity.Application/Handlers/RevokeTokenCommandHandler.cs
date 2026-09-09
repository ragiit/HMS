using MediatR;
using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;

namespace Identity.Application.Handlers;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;

    public RevokeTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ValidationException("RefreshToken", "Token cannot be empty.");

        var user = await _userRepository.GetByLoginAsync(_executionContext.UserName ?? string.Empty, cancellationToken);
        var token = user?.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);

        if (token is null || !token.IsActive)
            throw new NotFoundException("Refresh token is not found or already inactive.");

        token.Revoke("Revoked by user logout", _executionContext.IpAddress);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}