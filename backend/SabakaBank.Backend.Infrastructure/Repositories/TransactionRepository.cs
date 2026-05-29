using Microsoft.EntityFrameworkCore;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
        => await DbSet
            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}