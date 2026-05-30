using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Transactions.Queries.GetAccountTransactions;

public class GetAccountTransactionsQueryHandler : IRequestHandler<GetAccountTransactionsQuery, Result<IReadOnlyList<TransactionDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetAccountTransactionsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(GetAccountTransactionsQuery request, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        var transactions = await _uow.Transactions.GetByAccountIdAsync(request.AccountId, ct);

        var dtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.Type.ToString(),
            t.Status.ToString(),
            t.Amount,
            t.Currency.ToString(),
            t.ConvertedAmount,
            t.ConvertedCurrency?.ToString(),
            t.Description,
            t.FromAccountId,
            t.ToAccountId,
            t.CreatedAt)).ToList();

        return dtos;
    }
}
