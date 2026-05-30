namespace SabakaBank.Backend.Application.Cards.Queries.GetAccountCards;

public record CardDto(
    Guid     Id,
    string   MaskedNumber,
    string   HolderName,
    string   ExpiresAt,
    string   Type,
    string   Status,
    Guid     AccountId);
