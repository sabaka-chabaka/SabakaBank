using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Domain.ValueObjects;

namespace SabakaBank.Backend.Application.Accounts.Commands.Transfer;

public class TransferCommandHandler : IRequestHandler<TransferCommand, AppError?>
{
    private readonly IUnitOfWork _uow;

    public TransferCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppError?> Handle(TransferCommand request, CancellationToken ct)
    {
        if (request.FromAccountId == request.ToAccountId)
            return AppError.InvalidOperation("Cannot transfer to the same account.");

        var from = await _uow.Accounts.GetByIdAsync(request.FromAccountId, ct);
        if (from is null)
            return AppError.NotFound($"Account {request.FromAccountId} not found.");

        if (from.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        var to = await _uow.Accounts.GetByIdAsync(request.ToAccountId, ct);
        if (to is null)
            return AppError.NotFound($"Account {request.ToAccountId} not found.");

        try
        {
            var money = Money.Of(request.Amount, request.Currency);
            from.Withdraw(money);
            to.Deposit(money);
        }
        catch (Exception ex)
        {
            return AppError.InvalidOperation(ex.Message);
        }

        var tx = Transaction.CreateTransfer(from.Id, to.Id, request.Amount, request.Currency);
        tx.Complete();

        _uow.Accounts.Update(from);
        _uow.Accounts.Update(to);
        await _uow.Transactions.AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        return null;
    }
}
