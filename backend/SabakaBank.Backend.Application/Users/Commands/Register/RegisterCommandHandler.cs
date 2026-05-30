using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Users.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordService _passwords;

    public RegisterCommandHandler(IUnitOfWork uow, IPasswordService passwords)
    {
        _uow       = uow;
        _passwords = passwords;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _uow.Users.ExistsByEmailAsync(request.Email, ct))
            return AppError.Conflict($"Email '{request.Email}' is already taken.");

        if (await _uow.Users.ExistsByUsernameAsync(request.Username, ct))
            return AppError.Conflict($"Username '{request.Username}' is already taken.");

        var hash = _passwords.Hash(request.Password);
        var user = new User(request.Username, request.Email, hash, request.FirstName, request.LastName);

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return user.Id;
    }
}
