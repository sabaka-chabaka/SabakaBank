using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResultDto>>;
