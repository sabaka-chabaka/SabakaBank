using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Accounts.Commands.Transfer;

public record TransferCommand(
    Guid     FromAccountId,
    Guid     ToAccountId,
    Guid     RequestingUserId,
    decimal  Amount,
    Currency Currency) : IRequest<AppError?>;
