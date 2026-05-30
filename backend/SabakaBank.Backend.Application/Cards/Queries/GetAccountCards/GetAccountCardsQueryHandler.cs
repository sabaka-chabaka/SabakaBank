using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Cards.Queries.GetAccountCards;

public class GetAccountCardsQueryHandler : IRequestHandler<GetAccountCardsQuery, Result<IReadOnlyList<CardDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetAccountCardsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<CardDto>>> Handle(GetAccountCardsQuery request, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        var cards = await _uow.Cards.GetByAccountIdAsync(request.AccountId, ct);

        var dtos = cards.Select(c => new CardDto(
            c.Id,
            c.MaskedNumber,
            c.HolderName,
            c.ExpiresAt.ToString("MM/yy"),
            c.Type.ToString(),
            c.Status.ToString(),
            c.AccountId)).ToList();

        return dtos;
    }
}
