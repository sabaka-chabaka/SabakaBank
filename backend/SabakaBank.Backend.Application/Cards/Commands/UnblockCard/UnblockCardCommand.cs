using MediatR;
using SabakaBank.Backend.Application.Common.Errors;

namespace SabakaBank.Backend.Application.Cards.Commands.UnblockCard;

public record UnblockCardCommand(Guid CardId, Guid RequestingUserId) : IRequest<AppError?>;
