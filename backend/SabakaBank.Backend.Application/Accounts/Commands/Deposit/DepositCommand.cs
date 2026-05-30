using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Accounts.Commands.Deposit;

public record DepositCommand(
    Guid    AccountId,
    Guid    RequestingUserId,
    decimal Amount,
    Currency Currency) : IRequest<AppError?>;
