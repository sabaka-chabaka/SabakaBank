using MediatR;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, AppError?>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordService _passwords;

    public ChangePasswordCommandHandler(IUnitOfWork uow, IPasswordService passwords)
    {
        _uow       = uow;
        _passwords = passwords;
    }

    public async Task<AppError?> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return AppError.NotFound($"User {request.UserId} not found.");

        if (!_passwords.Verify(request.CurrentPassword, user.PasswordHash))
            return AppError.Forbidden("Current password is incorrect.");

        var newHash = _passwords.Hash(request.NewPassword);
        user.UpdatePassword(newHash);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return null;
    }
}