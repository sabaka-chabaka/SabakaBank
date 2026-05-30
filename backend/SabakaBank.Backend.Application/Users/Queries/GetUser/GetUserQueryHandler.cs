using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _uow;

    public GetUserQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return AppError.NotFound($"User {request.UserId} not found.");

        return new UserDto(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.IsActive);
    }
}
