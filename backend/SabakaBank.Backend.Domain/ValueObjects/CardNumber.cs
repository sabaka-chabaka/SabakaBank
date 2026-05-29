namespace SabakaBank.Backend.Domain.ValueObjects;

public sealed class CardNumber
{
    public string Value { get; }

    private CardNumber(string value)
    {
        Value = value;
    }

    public static CardNumber Generate()
    {
        var rng    = Random.Shared;
        var digits = new int[16];

        digits[0] = 4;
        for (var i = 1; i < 15; i++)
            digits[i] = rng.Next(0, 10);

        digits[15] = ComputeLuhnCheckDigit(digits);

        var raw = string.Concat(digits);
        return new CardNumber(Format(raw));
    }

    public static CardNumber From(string raw)
    {
        var digits = raw.Replace(" ", "").Replace("-", "");
        if (digits.Length != 16 || !digits.All(char.IsDigit))
            throw new ArgumentException("Invalid card number.");
        if (!ValidateLuhn(digits))
            throw new ArgumentException("Card number failed Luhn check.");
        return new CardNumber(Format(digits));
    }

    public string Masked => $"**** **** **** {Value[^4..]}";

    private static string Format(string raw) =>
        $"{raw[..4]} {raw[4..8]} {raw[8..12]} {raw[12..]}";

    private static int ComputeLuhnCheckDigit(int[] digits)
    {
        var sum = 0;
        for (var i = 0; i < 15; i++)
        {
            var d = digits[i];
            if (i % 2 == 0) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
        }
        return (10 - sum % 10) % 10;
    }

    private static bool ValidateLuhn(string number)
    {
        var sum = 0;
        var odd = true;
        for (var i = number.Length - 1; i >= 0; i--)
        {
            var d = number[i] - '0';
            if (odd) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
            odd = !odd;
        }
        return sum % 10 == 0;
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is CardNumber c && c.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}