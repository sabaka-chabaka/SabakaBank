using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Domain.Repositories;

public interface ICardRepository : IRepository<Card>
{
    Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Card>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}