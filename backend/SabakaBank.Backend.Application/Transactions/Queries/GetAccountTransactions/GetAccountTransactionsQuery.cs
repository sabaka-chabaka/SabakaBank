using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Transactions.Queries.GetAccountTransactions;

public record GetAccountTransactionsQuery(Guid AccountId, Guid RequestingUserId) : IRequest<Result<IReadOnlyList<TransactionDto>>>;
