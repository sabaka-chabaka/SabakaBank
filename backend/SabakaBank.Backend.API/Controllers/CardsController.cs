using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabakaBank.Backend.API.Extensions;
using SabakaBank.Backend.Application.Cards.Commands.BlockCard;
using SabakaBank.Backend.Application.Cards.Commands.IssueCard;
using SabakaBank.Backend.Application.Cards.Commands.UnblockCard;
using SabakaBank.Backend.Application.Cards.Queries.GetAccountCards;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts/{accountId:guid}/cards")]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IResult> GetCards(Guid accountId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetAccountCardsQuery(accountId, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }

    [HttpPost]
    public async Task<IResult> IssueCard(Guid accountId, [FromBody] IssueCardRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new IssueCardCommand(accountId, userId, request.Type), ct);
        return result.IsSuccess
            ? Results.Created($"/api/accounts/{accountId}/cards/{result.Value}", new { Id = result.Value })
            : ApiResponse.FromError(result.Error!);
    }

    [HttpPost("{cardId:guid}/block")]
    public async Task<IResult> BlockCard(Guid accountId, Guid cardId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new BlockCardCommand(cardId, userId), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }

    [HttpPost("{cardId:guid}/unblock")]
    public async Task<IResult> UnblockCard(Guid accountId, Guid cardId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new UnblockCardCommand(cardId, userId), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }
}

public record IssueCardRequest(CardType Type);
