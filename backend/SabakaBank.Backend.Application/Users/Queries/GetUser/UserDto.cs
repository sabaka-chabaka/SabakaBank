namespace SabakaBank.Backend.Application.Users.Queries.GetUser;

public record UserDto(
    Guid   Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool   IsActive);
