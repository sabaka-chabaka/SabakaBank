using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Accounts.Queries.GetAccount;

public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, Result<AccountDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAccountQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AccountDto>> Handle(GetAccountQuery request, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return AppError.NotFound($"Account {request.AccountId} not found.");

        if (account.UserId != request.RequestingUserId)
            return AppError.Forbidden("Access denied.");

        return new AccountDto(
            account.Id,
            account.AccountNumber,
            account.Type.ToString(),
            account.Currency.ToString(),
            account.Balance,
            account.IsActive,
            account.UserId,
            account.CreatedAt);
    }
}
