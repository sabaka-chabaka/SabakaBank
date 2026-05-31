using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.UnitTests;

internal static class EntityFactory
{
    public static User CreateUser(
        string username = "sabaka",
        string email = "sabaka@bank.com",
        string passwordHash = "hashed",
        string firstName = "Sabaka",
        string lastName = "Chabaka",
        bool active = true)
    {
        var user = new User(username, email, passwordHash, firstName, lastName);
        if (!active) user.Deactivate();
        return user;
    }

    public static Account CreateAccount(
        Guid? userId = null,
        AccountType type = AccountType.Checking,
        Currency currency = Currency.USD)
        => new(userId ?? Guid.NewGuid(), type, currency);

    public static Card CreateCard(
        Guid? accountId = null,
        CardType type = CardType.Debit)
        => new(accountId ?? Guid.NewGuid(), "SABAKA CHABAKA", type);
}
