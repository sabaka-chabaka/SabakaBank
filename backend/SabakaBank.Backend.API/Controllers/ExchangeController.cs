using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabakaBank.Backend.API.Extensions;
using SabakaBank.Backend.Application.Transactions.Commands.Exchange;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts/{accountId:guid}/exchange")]
public class ExchangeController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IResult> Exchange(Guid accountId, [FromBody] ExchangeRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new ExchangeCommand(accountId, userId, request.Amount, request.FromCurrency, request.ToCurrency), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }
}

public record ExchangeRequest(decimal Amount, Currency FromCurrency, Currency ToCurrency);
