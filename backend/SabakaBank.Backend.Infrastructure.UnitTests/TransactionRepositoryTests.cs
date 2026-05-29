using SabakaBank.Backend.Infrastructure.Persistence;
using SabakaBank.Backend.Infrastructure.Repositories;
using SabakaBank.Backend.Infrastructure.UnitTests;
using Xunit;

namespace SabakaBank.Backend.Infrastructure.UnitTests.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests()
    {
        _context    = DbContextFactory.Create();
        _repository = new TransactionRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_SavesTransaction()
    {
        var accountId = Guid.NewGuid();
        var tx = EntityFactory.CreateDeposit(accountId);

        await _repository.AddAsync(tx);
        await _context.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(tx.Id);
        Assert.NotNull(saved);
        Assert.Equal(tx.Amount, saved.Amount);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsTransactions_WhereFromAccount()
    {
        var accountId = Guid.NewGuid();
        await _repository.AddAsync(EntityFactory.CreateDeposit(accountId, 100m));
        await _repository.AddAsync(EntityFactory.CreateDeposit(accountId, 200m));
        await _repository.AddAsync(EntityFactory.CreateDeposit(Guid.NewGuid(), 50m));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByAccountIdAsync(accountId);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(accountId, t.FromAccountId));
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsTransactions_WhereToAccount()
    {
        var fromId = Guid.NewGuid();
        var toId   = Guid.NewGuid();
        await _repository.AddAsync(EntityFactory.CreateTransfer(fromId, toId));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByAccountIdAsync(toId);

        Assert.Single(result);
        Assert.Equal(toId, result[0].ToAccountId);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsBothSidesOfTransfer()
    {
        var fromId = Guid.NewGuid();
        var toId   = Guid.NewGuid();
        await _repository.AddAsync(EntityFactory.CreateTransfer(fromId, toId));
        await _context.SaveChangesAsync();

        var fromResults = await _repository.GetByAccountIdAsync(fromId);
        var toResults   = await _repository.GetByAccountIdAsync(toId);

        Assert.Single(fromResults);
        Assert.Single(toResults);
        Assert.Equal(fromResults[0].Id, toResults[0].Id);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsEmpty_WhenNoTransactions()
    {
        var result = await _repository.GetByAccountIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsOrderedByCreatedAtDescending()
    {
        var accountId = Guid.NewGuid();
        var tx1 = EntityFactory.CreateDeposit(accountId, 10m);
        var tx2 = EntityFactory.CreateDeposit(accountId, 20m);
        var tx3 = EntityFactory.CreateDeposit(accountId, 30m);

        await _repository.AddAsync(tx1);
        await _repository.AddAsync(tx2);
        await _repository.AddAsync(tx3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByAccountIdAsync(accountId);

        var dates = result.Select(t => t.CreatedAt).ToList();
        Assert.Equal(dates.OrderByDescending(d => d).ToList(), dates);
    }

    [Fact]
    public async Task Delete_RemovesTransaction()
    {
        var tx = EntityFactory.CreateDeposit(Guid.NewGuid());
        await _repository.AddAsync(tx);
        await _context.SaveChangesAsync();

        _repository.Delete(tx);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(tx.Id);
        Assert.Null(result);
    }
}
