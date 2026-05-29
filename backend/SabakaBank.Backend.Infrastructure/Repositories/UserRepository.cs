using Microsoft.EntityFrameworkCore;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.AnyAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => await DbSet.AnyAsync(u => u.Username == username, ct);
}