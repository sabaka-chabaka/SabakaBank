using SabakaBank.Backend.Domain.Common;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Domain.Entities;

public class Transaction : BaseEntity
{
    public TransactionType   Type              { get; private set; }
    public TransactionStatus Status            { get; private set; }
    public decimal           Amount            { get; private set; }
    public Currency          Currency          { get; private set; }
    public decimal?          ConvertedAmount   { get; private set; }
    public Currency?         ConvertedCurrency { get; private set; }
    public string?           Description       { get; private set; }

    public Guid     FromAccountId { get; private set; }
    public Account  FromAccount   { get; private set; } = null!;

    public Guid?    ToAccountId { get; private set; }
    public Account? ToAccount   { get; private set; }

    private Transaction() { }

    private Transaction(
        TransactionType type,
        Guid fromAccountId,
        decimal amount,
        Currency currency,
        Guid? toAccountId,
        decimal? convertedAmount,
        Currency? convertedCurrency,
        string? description)
    {
        Type              = type;
        FromAccountId     = fromAccountId;
        ToAccountId       = toAccountId;
        Amount            = amount;
        Currency          = currency;
        ConvertedAmount   = convertedAmount;
        ConvertedCurrency = convertedCurrency;
        Description       = description;
        Status            = TransactionStatus.Pending;
    }

    public static Transaction CreateDeposit(Guid accountId, decimal amount, Currency currency, string? description = null)
        => new(TransactionType.Deposit, accountId, amount, currency, null, null, null, description);

    public static Transaction CreateWithdrawal(Guid accountId, decimal amount, Currency currency, string? description = null)
        => new(TransactionType.Withdrawal, accountId, amount, currency, null, null, null, description);

    public static Transaction CreateTransfer(Guid fromAccountId, Guid toAccountId, decimal amount, Currency currency, string? description = null)
        => new(TransactionType.Transfer, fromAccountId, amount, currency, toAccountId, null, null, description);

    public static Transaction CreateExchange(Guid accountId, decimal amount, Currency from, decimal convertedAmount, Currency to, string? description = null)
        => new(TransactionType.Exchange, accountId, amount, from, null, convertedAmount, to, description);

    public void Complete()
    {
        Status = TransactionStatus.Completed;
        Touch();
    }

    public void Fail()
    {
        Status = TransactionStatus.Failed;
        Touch();
    }

    public void Cancel()
    {
        Status = TransactionStatus.Cancelled;
        Touch();
    }
}