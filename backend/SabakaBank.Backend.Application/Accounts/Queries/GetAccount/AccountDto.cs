namespace SabakaBank.Backend.Application.Accounts.Queries.GetAccount;

public record AccountDto(
    Guid     Id,
    string   AccountNumber,
    string   Type,
    string   Currency,
    decimal  Balance,
    bool     IsActive,
    Guid     UserId,
    DateTime CreatedAt);
