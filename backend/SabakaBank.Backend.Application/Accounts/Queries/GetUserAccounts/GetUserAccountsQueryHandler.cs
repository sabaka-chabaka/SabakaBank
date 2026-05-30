using MediatR;
using SabakaBank.Backend.Application.Accounts.Queries.GetAccount;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Accounts.Queries.GetUserAccounts;

public class GetUserAccountsQueryHandler : IRequestHandler<GetUserAccountsQuery, Result<IReadOnlyList<AccountDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetUserAccountsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<AccountDto>>> Handle(GetUserAccountsQuery request, CancellationToken ct)
    {
        var accounts = await _uow.Accounts.GetByUserIdAsync(request.UserId, ct);

        var dtos = accounts.Select(a => new AccountDto(
            a.Id,
            a.AccountNumber,
            a.Type.ToString(),
            a.Currency.ToString(),
            a.Balance,
            a.IsActive,
            a.UserId,
            a.CreatedAt)).ToList();

        return dtos;
    }
}
