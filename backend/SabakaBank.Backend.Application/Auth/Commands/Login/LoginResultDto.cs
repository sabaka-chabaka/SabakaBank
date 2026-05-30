namespace SabakaBank.Backend.Application.Auth.Commands.Login;

public record LoginResultDto(Guid UserId, string Token);
