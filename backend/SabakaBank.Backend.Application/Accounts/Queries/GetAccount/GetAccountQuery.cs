using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Accounts.Queries.GetAccount;

public record GetAccountQuery(Guid AccountId, Guid RequestingUserId) : IRequest<Result<AccountDto>>;
