using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Cards.Commands.UnblockCard;

public class UnblockCardCommandHandler : IRequestHandler<UnblockCardCommand, AppError?>
{
    private readonly IUnitOfWork _uow;

    public UnblockCardCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppError?> Handle(UnblockCardCommand request, CancellationToken ct)
    {
        var card = await _uow.Cards.GetByIdAsync(request.CardId, ct);
        if (card is null)
            return AppError.NotFound($"Card {request.CardId} not found.");

        var account = await _uow.Accounts.GetByIdAsync(card.AccountId, ct);
        if (account is null || account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        try
        {
            card.Unblock();
        }
        catch (Exception ex)
        {
            return AppError.InvalidOperation(ex.Message);
        }

        _uow.Cards.Update(card);
        await _uow.SaveChangesAsync(ct);

        return null;
    }
}
