using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabakaBank.Backend.API.Extensions;
using SabakaBank.Backend.Application.Users.Commands.ChangePassword;
using SabakaBank.Backend.Application.Users.Commands.Register;
using SabakaBank.Backend.Application.Users.Queries.GetUser;

namespace SabakaBank.Backend.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value}", new { Id = result.Value })
            : ApiResponse.FromError(result.Error!);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IResult> GetMe(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetUserQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }

    [Authorize]
    [HttpPut("me/password")]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
