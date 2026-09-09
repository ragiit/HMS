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

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExecutionContext _executionContext;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IExecutionContext executionContext,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _executionContext = executionContext;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByLoginAsync(
            request.UsernameOrEmail,
            cancellationToken);

        if (user is null || user.IsDeleted)
            throw new UnauthorizedException("Invalid credentials.");

        if (!user.IsActive)
            throw new PermissionDeniedException(
                "Your account is disabled. Please contact your administrator.");

        if (user.CheckIsLockedOut())
        {
            var remainingMinutes = user.LockoutEndDate.HasValue
                ? Math.Max(
                    1,
                    (int)Math.Ceiling(
                        (user.LockoutEndDate.Value - DateTimeOffset.UtcNow).TotalMinutes))
                : 15;

            throw new PermissionDeniedException(
                $"Account is temporarily locked due to multiple failed login attempts. " +
                $"Please try again in {remainingMinutes} minute(s).");
        }

        var isPasswordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Invalid credentials.");
        }

        user.RecordLogin(_executionContext.IpAddress);

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

        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshExpiryDays =
            int.TryParse(
                _configuration["Jwt:RefreshTokenExpiryDays"],
                out var days)
                ? days
                : 7;

        var expiresOn = DateTimeOffset.UtcNow.AddDays(refreshExpiryDays);

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenString,
            expiresOn,
            clientId: request.ClientId,
            createdByIp: _executionContext.IpAddress
        );

        user.RefreshTokens.Add(refreshToken);

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
            RefreshToken = refreshToken.Token,
            ExpiresIn = tokenResult.ExpiresInSeconds,
            User = userDto,
            TokenType = "Bearer"
        };
    }
}