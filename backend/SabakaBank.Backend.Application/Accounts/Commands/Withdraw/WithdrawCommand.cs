using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Accounts.Commands.Withdraw;

public record WithdrawCommand(
    Guid     AccountId,
    Guid     RequestingUserId,
    decimal  Amount,
    Currency Currency) : IRequest<AppError?>;
