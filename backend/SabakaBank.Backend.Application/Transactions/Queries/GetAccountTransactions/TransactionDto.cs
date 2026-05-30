namespace SabakaBank.Backend.Application.Transactions.Queries.GetAccountTransactions;

public record TransactionDto(
    Guid      Id,
    string    Type,
    string    Status,
    decimal   Amount,
    string    Currency,
    decimal?  ConvertedAmount,
    string?   ConvertedCurrency,
    string?   Description,
    Guid      FromAccountId,
    Guid?     ToAccountId,
    DateTime  CreatedAt);
