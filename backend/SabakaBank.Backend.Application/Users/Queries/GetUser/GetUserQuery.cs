using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Users.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<Result<UserDto>>;
