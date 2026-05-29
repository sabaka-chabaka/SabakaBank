using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Domain.ValueObjects;

public sealed class Money
{
    public const decimal SC_TO_USD_RATE = 1.5m;

    public decimal  Amount   { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        Amount   = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public static Money Of(decimal amount, Currency currency) => new(amount, currency);
    public static Money Zero(Currency currency) => new(0m, currency);

    public Money ConvertTo(Currency target)
    {
        if (Currency == target) return this;

        var converted = Currency == Currency.SC
            ? Amount * SC_TO_USD_RATE
            : Amount / SC_TO_USD_RATE;

        return new Money(converted, target);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}.");
    }

    public override string ToString() => $"{Amount} {Currency}";

    public override bool Equals(object? obj) =>
        obj is Money m && m.Amount == Amount && m.Currency == Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}