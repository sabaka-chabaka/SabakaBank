using SabakaBank.Backend.Domain.Common;
using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Domain.ValueObjects;

namespace SabakaBank.Backend.Domain.Entities;

public class Account : BaseEntity
{
    public string      AccountNumber { get; private set; }
    public AccountType Type          { get; private set; }
    public Currency    Currency      { get; private set; }
    public decimal     Balance       { get; private set; }
    public bool        IsActive      { get; private set; }

    public Guid UserId { get; private set; }
    public User User   { get; private set; } = null!;

    private readonly List<Card>        _cards        = [];
    private readonly List<Transaction> _transactions = [];

    public IReadOnlyCollection<Card>        Cards        => _cards.AsReadOnly();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { }

    public Account(Guid userId, AccountType type, Currency currency)
    {
        UserId        = userId;
        Type          = type;
        Currency      = currency;
        Balance       = 0m;
        IsActive      = true;
        AccountNumber = GenerateAccountNumber();
    }

    public Money GetBalance() => Money.Of(Balance, Currency);

    public void Deposit(Money amount)
    {
        EnsureActive();
        EnsureSameCurrency(amount);
        Balance = GetBalance().Add(amount).Amount;
        Touch();
    }

    public void Withdraw(Money amount)
    {
        EnsureActive();
        EnsureSameCurrency(amount);

        var balance = GetBalance();
        if (!balance.IsGreaterThanOrEqual(amount))
            throw new InvalidOperationException($"Insufficient funds: have {balance}, need {amount}.");

        Balance = balance.Subtract(amount).Amount;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Account is not active.");
    }

    private void EnsureSameCurrency(Money money)
    {
        if (money.Currency != Currency)
            throw new InvalidOperationException($"Account currency is {Currency}, got {money.Currency}.");
    }

    private static string GenerateAccountNumber()
    {
        var datePart   = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Random.Shared.Next(10_000_000, 99_999_999);
        return $"SB-{datePart}-{randomPart}";
    }
}