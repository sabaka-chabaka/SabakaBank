using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.Cards.Commands.IssueCard;

public record IssueCardCommand(
    Guid     AccountId,
    Guid     RequestingUserId,
    CardType Type) : IRequest<Result<Guid>>;
