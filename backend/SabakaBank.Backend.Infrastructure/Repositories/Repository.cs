using Microsoft.EntityFrameworkCore;
using SabakaBank.Backend.Domain.Common;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet   = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => DbSet.Update(entity);

    public void Delete(T entity)
        => DbSet.Remove(entity);
}