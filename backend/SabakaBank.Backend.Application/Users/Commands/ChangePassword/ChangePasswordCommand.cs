using MediatR;
using SabakaBank.Backend.Application.Common.Errors;

namespace SabakaBank.Backend.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
    Guid   UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<AppError?>;