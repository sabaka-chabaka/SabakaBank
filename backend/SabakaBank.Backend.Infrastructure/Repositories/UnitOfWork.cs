using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IUserRepository        Users        { get; }
    public IAccountRepository     Accounts     { get; }
    public ICardRepository        Cards        { get; }
    public ITransactionRepository Transactions { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context     = context;
        Users        = new UserRepository(context);
        Accounts     = new AccountRepository(context);
        Cards        = new CardRepository(context);
        Transactions = new TransactionRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}