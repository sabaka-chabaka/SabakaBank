using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Cards.Queries.GetAccountCards;

public record GetAccountCardsQuery(Guid AccountId, Guid RequestingUserId) : IRequest<Result<IReadOnlyList<CardDto>>>;
