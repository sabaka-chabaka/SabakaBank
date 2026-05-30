using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Accounts.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public CreateAccountCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return AppError.NotFound($"User {request.UserId} not found.");

        if (!user.IsActive)
            return AppError.Forbidden("User is deactivated.");

        var account = new Account(request.UserId, request.Type, request.Currency);

        await _uow.Accounts.AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);

        return account.Id;
    }
}
