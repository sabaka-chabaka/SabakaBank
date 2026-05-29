using System.Security.Cryptography;
using System.Text;

namespace SabakaBank.Backend.Domain.ValueObjects;

public sealed class Cvv
{
    public string Hash { get; }

    private Cvv(string hash)
    {
        Hash = hash;
    }

    public static Cvv Generate()
    {
        var raw = Random.Shared.Next(100, 1000).ToString();
        return new Cvv(ComputeHash(raw));
    }

    public static Cvv FromHash(string hash) => new(hash);

    public bool Verify(string raw) => ComputeHash(raw) == Hash;

    private static string ComputeHash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}