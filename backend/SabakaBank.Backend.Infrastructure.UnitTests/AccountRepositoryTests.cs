using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Infrastructure.Persistence;
using SabakaBank.Backend.Infrastructure.Repositories;
using SabakaBank.Backend.Infrastructure.UnitTests;
using Xunit;

namespace SabakaBank.Backend.Infrastructure.UnitTests.Repositories;

public class AccountRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AccountRepository _repository;

    public AccountRepositoryTests()
    {
        _context    = DbContextFactory.Create();
        _repository = new AccountRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_SavesAccount()
    {
        var account = EntityFactory.CreateAccount();

        await _repository.AddAsync(account);
        await _context.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(account.Id);
        Assert.NotNull(saved);
        Assert.Equal(account.AccountNumber, saved.AccountNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAccountNumberAsync_ReturnsAccount_WhenExists()
    {
        var account = EntityFactory.CreateAccount();
        await _repository.AddAsync(account);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByAccountNumberAsync(account.AccountNumber);

        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
    }

    [Fact]
    public async Task GetByAccountNumberAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByAccountNumberAsync("SB-00000000-00000000");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllUserAccounts()
    {
        var userId = Guid.NewGuid();
        await _repository.AddAsync(EntityFactory.CreateAccount(userId, AccountType.Checking, Currency.USD));
        await _repository.AddAsync(EntityFactory.CreateAccount(userId, AccountType.Savings, Currency.SC));
        await _repository.AddAsync(EntityFactory.CreateAccount(Guid.NewGuid()));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(userId, a.UserId));
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoAccounts()
    {
        var result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdWithCardsAsync_ReturnsAccountWithCards()
    {
        var account = EntityFactory.CreateAccount();
        await _repository.AddAsync(account);
        await _context.SaveChangesAsync();

        var card = EntityFactory.CreateCard(account.Id);
        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdWithCardsAsync(account.Id);

        Assert.NotNull(result);
        Assert.Single(result.Cards);
    }

    [Fact]
    public async Task GetByIdWithCardsAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdWithCardsAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithTransactionsAsync_ReturnsAccountWithTransactions()
    {
        var account = EntityFactory.CreateAccount();
        await _repository.AddAsync(account);
        await _context.SaveChangesAsync();

        var tx = EntityFactory.CreateDeposit(account.Id);
        await _context.Transactions.AddAsync(tx);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdWithTransactionsAsync(account.Id);

        Assert.NotNull(result);
        Assert.Single(result.Transactions);
    }

    [Fact]
    public async Task Delete_RemovesAccount()
    {
        var account = EntityFactory.CreateAccount();
        await _repository.AddAsync(account);
        await _context.SaveChangesAsync();

        _repository.Delete(account);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(account.Id);
        Assert.Null(result);
    }
}
