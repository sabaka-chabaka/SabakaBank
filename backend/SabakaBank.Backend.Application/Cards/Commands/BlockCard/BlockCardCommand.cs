using MediatR;
using SabakaBank.Backend.Application.Common.Errors;

namespace SabakaBank.Backend.Application.Cards.Commands.BlockCard;

public record BlockCardCommand(Guid CardId, Guid RequestingUserId) : IRequest<AppError?>;
