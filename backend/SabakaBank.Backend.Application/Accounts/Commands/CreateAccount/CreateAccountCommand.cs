using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    Guid        UserId,
    AccountType Type,
    Currency    Currency) : IRequest<Result<Guid>>;
