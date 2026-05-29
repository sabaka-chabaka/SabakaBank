using SabakaBank.Backend.Domain.Entities;

namespace SabakaBank.Backend.Domain.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Account?> GetByIdWithCardsAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);
}