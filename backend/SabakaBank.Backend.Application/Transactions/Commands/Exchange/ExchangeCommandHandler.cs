using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Domain.ValueObjects;

namespace SabakaBank.Backend.Application.Transactions.Commands.Exchange;

public class ExchangeCommandHandler : IRequestHandler<ExchangeCommand, AppError?>
{
    private readonly IUnitOfWork _uow;

    public ExchangeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppError?> Handle(ExchangeCommand request, CancellationToken ct)
    {
        if (request.FromCurrency == request.ToCurrency)
            return AppError.InvalidOperation("From and To currencies must differ.");

        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        if (account.Currency != request.FromCurrency)
            return AppError.InvalidOperation($"Account currency is {account.Currency}, not {request.FromCurrency}.");

        Money sourceAmount;
        Money convertedAmount;

        try
        {
            sourceAmount    = Money.Of(request.Amount, request.FromCurrency);
            convertedAmount = sourceAmount.ConvertTo(request.ToCurrency);
            account.Withdraw(sourceAmount);
        }
        catch (Exception ex)
        {
            return AppError.InvalidOperation(ex.Message);
        }

        var toAccount = await _uow.Accounts.GetByUserIdAsync(request.RequestingUserId, ct)
            .ContinueWith(t => t.Result.FirstOrDefault(a => a.Currency == request.ToCurrency && a.IsActive), ct);

        if (toAccount is null)
            return AppError.NotFound($"No active {request.ToCurrency} account found for this user.");

        try
        {
            toAccount.Deposit(convertedAmount);
        }
        catch (Exception ex)
        {
            return AppError.InvalidOperation(ex.Message);
        }

        var tx = Transaction.CreateExchange(
            account.Id,
            sourceAmount.Amount,
            request.FromCurrency,
            convertedAmount.Amount,
            request.ToCurrency);

        tx.Complete();

        _uow.Accounts.Update(account);
        _uow.Accounts.Update(toAccount);
        await _uow.Transactions.AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        return null;
    }
}
