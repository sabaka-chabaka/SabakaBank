using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}