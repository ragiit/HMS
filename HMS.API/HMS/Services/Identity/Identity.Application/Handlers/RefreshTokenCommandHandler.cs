using MediatR;
using Microsoft.Extensions.Configuration;
using HMS.Shared.Abstractions;
using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Handlers;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new UnauthorizedException("Refresh token cannot be empty.");

        var user = await _userRepository.GetByLoginAsync(
            _executionContext.UserName ?? string.Empty,
            cancellationToken);

        // Fallback: If username claim is missing from execution context, find token directly
        var existingToken = user?.RefreshTokens
            .FirstOrDefault(t => t.Token == request.RefreshToken);

        if (user is null || existingToken is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (!user.IsActive || user.IsDeleted)
            throw new PermissionDeniedException("Your account is disabled.");

        if (existingToken.IsRevoked)
        {
            // Possible token reuse attack: revoke all tokens for safety
            user.RevokeAllRefreshTokens(
                "Attempted reuse of revoked token",
                _executionContext.IpAddress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(
                "Refresh token has already been revoked. Please log in again.");
        }

        if (existingToken.IsExpired)
            throw new UnauthorizedException(
                "Refresh token has expired. Please log in again.");

        // 1. Generate new tokens
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshExpiryDays =
            int.TryParse(
                _configuration["Jwt:RefreshTokenExpiryDays"],
                out var days)
                ? days
                : 7;

        var newExpiresOn =
            DateTimeOffset.UtcNow.AddDays(refreshExpiryDays);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenString,
            newExpiresOn,
            clientId: request.ClientId,
            createdByIp: _executionContext.IpAddress
        );

        // 2. Revoke previous token and link replacement
        existingToken.Revoke(
            "Replaced by new token",
            _executionContext.IpAddress,
            newRefreshTokenString);

        user.RefreshTokens.Add(newRefreshToken);

        // 3. Generate new access token
        var roleNames = user.Roles
            .Where(r => r.Role is not null)
            .Select(r => r.Role.Name)
            .ToList();

        var tokenUser = new TokenUser
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email
        };

        var tokenResult = _tokenService.GenerateAccessToken(
            tokenUser,
            roleNames);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roleNames
        };

        return new AuthResultDto
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = tokenResult.ExpiresInSeconds,
            User = userDto,
            TokenType = "Bearer"
        };
    }
}