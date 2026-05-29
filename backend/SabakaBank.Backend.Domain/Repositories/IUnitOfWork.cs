namespace SabakaBank.Backend.Domain.Repositories;

public interface IUnitOfWork
{
    IUserRepository        Users        { get; }
    IAccountRepository     Accounts     { get; }
    ICardRepository        Cards        { get; }
    ITransactionRepository Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}