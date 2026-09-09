using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMS.Shared.Abstractions;
using Identity.Application.Commands;
using Identity.Application.DTOs;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExecutionContext _executionContext;

    public AuthController(IMediator mediator, IExecutionContext executionContext)
    {
        _mediator = mediator;
        _executionContext = executionContext;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Token refreshed successfully."));
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Revoke(
        [FromBody] RevokeTokenCommand command,
        CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(message: "Refresh token revoked successfully."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromBody] ChangePasswordRequestDto dto,
        CancellationToken ct)
    {
        if (!Guid.TryParse(_executionContext.UserId, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid authentication token claims."));

        var command = new ChangePasswordCommand(userId, dto.CurrentPassword, dto.NewPassword);
        await _mediator.Send(command, ct);

        return Ok(ApiResponse.Ok(message: "Password changed successfully. Active sessions have been invalidated."));
    }
}

public sealed record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);