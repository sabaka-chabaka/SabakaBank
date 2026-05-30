using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Cards.Commands.IssueCard;

public class IssueCardCommandHandler : IRequestHandler<IssueCardCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public IssueCardCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(IssueCardCommand request, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        if (!account.IsActive)
            return AppError.InvalidOperation("Account is not active.");

        var user = await _uow.Users.GetByIdAsync(account.UserId, ct);
        if (user is null)
            return AppError.NotFound($"User {account.UserId} not found.");

        var holderName = $"{user.FirstName} {user.LastName}";
        var card = new Card(account.Id, holderName, request.Type);

        await _uow.Cards.AddAsync(card, ct);
        await _uow.SaveChangesAsync(ct);

        return card.Id;
    }
}
