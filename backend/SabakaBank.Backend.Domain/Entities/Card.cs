using SabakaBank.Backend.Domain.Common;
using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Domain.ValueObjects;

namespace SabakaBank.Backend.Domain.Entities;

public class Card : BaseEntity
{
    public string     CardNumber  { get; private set; }
    public string     CvvHash     { get; private set; }
    public string     HolderName  { get; private set; }
    public DateOnly   ExpiresAt   { get; private set; }
    public CardType   Type        { get; private set; }
    public CardStatus Status      { get; private set; }

    public Guid    AccountId { get; private set; }
    public Account Account   { get; private set; } = null!;

    private Card() { }

    public Card(Guid accountId, string holderName, CardType type)
    {
        AccountId  = accountId;
        HolderName = holderName.ToUpperInvariant();
        Type       = type;
        Status     = CardStatus.Active;

        var number = ValueObjects.CardNumber.Generate();
        var cvv    = ValueObjects.Cvv.Generate();

        CardNumber = number.Value;
        CvvHash    = cvv.Hash;
        ExpiresAt  = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(4));
    }

    public bool IsExpired() => DateOnly.FromDateTime(DateTime.UtcNow) > ExpiresAt;

    public void Block()
    {
        if (Status == CardStatus.Closed)
            throw new InvalidOperationException("Card is closed.");
        Status = CardStatus.Blocked;
        Touch();
    }

    public void Unblock()
    {
        if (Status != CardStatus.Blocked)
            throw new InvalidOperationException("Card is not blocked.");
        if (IsExpired())
            throw new InvalidOperationException("Card is expired.");
        Status = CardStatus.Active;
        Touch();
    }

    public void Close()
    {
        Status = CardStatus.Closed;
        Touch();
    }

    public void MarkExpired()
    {
        Status = CardStatus.Expired;
        Touch();
    }

    public string MaskedNumber => $"**** **** **** {CardNumber[^4..]}";
}