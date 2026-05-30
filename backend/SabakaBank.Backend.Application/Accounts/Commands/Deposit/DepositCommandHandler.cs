using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Domain.ValueObjects;

namespace SabakaBank.Backend.Application.Accounts.Commands.Deposit;

public class DepositCommandHandler : IRequestHandler<DepositCommand, AppError?>
{
    private readonly IUnitOfWork _uow;

    public DepositCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppError?> Handle(DepositCommand request, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        try
        {
            var money = Money.Of(request.Amount, request.Currency);
            account.Deposit(money);
        }
        catch (Exception ex)
        {
            return AppError.InvalidOperation(ex.Message);
        }

        var tx = Transaction.CreateDeposit(account.Id, request.Amount, request.Currency);
        tx.Complete();

        _uow.Accounts.Update(account);
        await _uow.Transactions.AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        return null;
    }
}
