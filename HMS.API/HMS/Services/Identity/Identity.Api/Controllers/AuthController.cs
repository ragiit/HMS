using HMS.Identity.Application.Commands;
using HMS.Identity.Application.DTOs;
using HMS.Shared.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Login berhasil"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Refresh(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Token diperbarui"));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Revoke(RevokeTokenCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(message: "Refresh token dicabut"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(message: "Password berhasil diubah"));
    }
}