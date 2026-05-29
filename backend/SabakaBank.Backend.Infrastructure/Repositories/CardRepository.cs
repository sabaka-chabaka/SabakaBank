using Microsoft.EntityFrameworkCore;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context) { }

    public async Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.CardNumber == cardNumber, ct);

    public async Task<IReadOnlyList<Card>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
        => await DbSet.Where(c => c.AccountId == accountId).ToListAsync(ct);
}