using HMS.Identity.Application.Abstractions;
using HMS.Identity.Application.Commands;
using HMS.Identity.Application.DTOs;
using HMS.Identity.Domain.Entities;
using HMS.Shared.Abstractions.Exceptions;
using HMS.Shared.Abstractions.Persistence;
using MediatR;

namespace HMS.Identity.Application.Handlers;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IGenericRepository<RefreshToken> _refreshTokens;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IGenericRepository<RefreshToken> refreshTokens,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _refreshTokens = refreshTokens;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var stored = await _refreshTokens.FirstOrDefaultAsync(
            t => t.Token == request.RefreshToken,
            ct);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedException("Refresh token tidak valid.");

        var user = await _userRepository.GetByIdWithRolesAsync(stored.UserId, ct);
        if (user is null || !user.IsActive || user.IsDeleted || user.IsLockedOut)
            throw new UnauthorizedException("User tidak aktif.");

        var roles = user.Roles.Where(r => r.Role is not null && r.Role.IsActive).Select(r => r.Role!.Name).ToList();

        var token = _tokenService.GenerateAccessToken(new TokenUser
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email
        }, roles);

        // Rotate refresh token
        var newRefresh = _tokenService.GenerateRefreshToken();
        stored.Revoke("rotated", newRefresh);
        _refreshTokens.Update(stored);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResultDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = newRefresh,
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