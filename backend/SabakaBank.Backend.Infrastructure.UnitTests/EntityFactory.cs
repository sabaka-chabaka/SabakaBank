using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Infrastructure.UnitTests;

internal static class EntityFactory
{
    public static User CreateUser(
        string username = "testuser",
        string email = "test@sabakabank.com",
        string passwordHash = "hash",
        string firstName = "Test",
        string lastName = "User")
        => new(username, email, passwordHash, firstName, lastName);

    public static Account CreateAccount(
        Guid? userId = null,
        AccountType type = AccountType.Checking,
        Currency currency = Currency.USD)
        => new(userId ?? Guid.NewGuid(), type, currency);

    public static Card CreateCard(
        Guid? accountId = null,
        string holderName = "TEST USER",
        CardType type = CardType.Debit)
        => new(accountId ?? Guid.NewGuid(), holderName, type);

    public static Transaction CreateDeposit(Guid accountId, decimal amount = 100m, Currency currency = Currency.USD)
        => Transaction.CreateDeposit(accountId, amount, currency);

    public static Transaction CreateTransfer(Guid fromId, Guid toId, decimal amount = 50m, Currency currency = Currency.USD)
        => Transaction.CreateTransfer(fromId, toId, amount, currency);
}
