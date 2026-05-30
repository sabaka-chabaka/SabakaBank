using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Transactions.Commands.Exchange;

public record ExchangeCommand(
    Guid     AccountId,
    Guid     RequestingUserId,
    decimal  Amount,
    Currency FromCurrency,
    Currency ToCurrency) : IRequest<AppError?>;
