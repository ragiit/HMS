using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using Identity.Application.Abstractions;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Handlers;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IGenericRepository<RefreshToken> _refreshTokens;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserRepository userRepository,
        IGenericRepository<RefreshToken> refreshTokens)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _refreshTokens = refreshTokens;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByLoginAsync(request.Username, ct);
        if (user is null || !user.IsActive || user.IsDeleted)
            throw new UnauthorizedException("Invalid username or password");

        if (user.IsLockedOut && user.LockoutEndDate <= DateTimeOffset.UtcNow)
            user.ResetFailedLogin();

        if (user.IsLockedOut)
            throw new UnauthorizedException("Account is locked. Try again later.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid username or password");
        }

        user.ResetFailedLogin();
        user.RecordLogin();

        var roles = user.Roles.Where(r => r.Role is not null && r.Role.IsActive).Select(r => r.Role!.Name).ToList();

        var token = _tokenService.GenerateAccessToken(new TokenUser
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email
        }, roles);

        var refresh = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken(user.Id, refresh, DateTimeOffset.UtcNow.AddDays(7), request.ClientId);
        await _refreshTokens.AddAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResultDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = refresh,
            ExpiresIn = token.ExpiresInSeconds,
            TokenType = "Bearer",
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Roles = roles
            }
        };
    }
}