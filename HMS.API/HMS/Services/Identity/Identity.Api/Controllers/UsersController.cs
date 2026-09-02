using HMS.Identity.Application.Commands;
using HMS.Identity.Application.DTOs;
using HMS.Identity.Application.Queries;
using HMS.Shared.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(page, pageSize, search), ct);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        if (result is null)
            return NotFound();
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Register(RegisterUserCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "User dibuat"));
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<ApiResponse>> AssignRoles(Guid id, AssignRolesCommand command, CancellationToken ct)
    {
        if (command.UserId != id)
            return BadRequest();
        await _mediator.Send(command with { UserId = id }, ct);
        return Ok(ApiResponse.Ok(message: "Roles diperbarui"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Update(Guid id, UpdateUserProfileCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { UserId = id }, ct);
        return Ok(ApiResponse.Ok(message: "Profil diperbarui"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateUserCommand(id), ct);
        return Ok(ApiResponse.Ok(message: "User dinonaktifkan"));
    }

    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetRoles(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllRolesQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(result));
    }
}