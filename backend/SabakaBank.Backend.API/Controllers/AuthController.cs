using MediatR;
using Microsoft.AspNetCore.Mvc;
using SabakaBank.Backend.API.Extensions;
using SabakaBank.Backend.Application.Auth.Commands.Login;

namespace SabakaBank.Backend.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }
}
