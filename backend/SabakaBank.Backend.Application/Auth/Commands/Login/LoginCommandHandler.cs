using MediatR;
using SabakaBank.Backend.Application.Common;
using SabakaBank.Backend.Application.Common.Errors;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordService _passwords;
    private readonly IJwtService      _jwt;

    public LoginCommandHandler(IUnitOfWork uow, IPasswordService passwords, IJwtService jwt)
    {
        _uow       = uow;
        _passwords = passwords;
        _jwt       = jwt;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct);
        if (user is null || !_passwords.Verify(request.Password, user.PasswordHash))
            return AppError.Forbidden("Invalid email or password.");

        if (!user.IsActive)
            return AppError.Forbidden("Account is deactivated.");

        var token = _jwt.GenerateToken(user);
        return new LoginResultDto(user.Id, token);
    }
}
