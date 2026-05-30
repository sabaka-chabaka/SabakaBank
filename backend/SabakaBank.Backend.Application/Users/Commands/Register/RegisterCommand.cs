using MediatR;
using SabakaBank.Backend.Application.Common;

namespace SabakaBank.Backend.Application.Users.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<Guid>>;