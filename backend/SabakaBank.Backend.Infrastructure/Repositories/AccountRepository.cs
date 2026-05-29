using Microsoft.EntityFrameworkCore;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context) { }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

    public async Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.Where(a => a.UserId == userId).ToListAsync(ct);

    public async Task<Account?> GetByIdWithCardsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(a => a.Cards).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Account?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Id == id, ct);
}