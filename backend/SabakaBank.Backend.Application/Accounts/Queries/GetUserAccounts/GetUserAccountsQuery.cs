using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Accounts.Queries.GetAccount;

namespace SabakaBank.Backend.Application.Accounts.Queries.GetUserAccounts;

public record GetUserAccountsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<AccountDto>>>;
