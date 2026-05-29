using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Infrastructure.Persistence;
using SabakaBank.Backend.Infrastructure.Repositories;
using SabakaBank.Backend.Infrastructure.UnitTests;
using Xunit;

namespace SabakaBank.Backend.Infrastructure.UnitTests.Repositories;

public class CardRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CardRepository _repository;

    public CardRepositoryTests()
    {
        _context    = DbContextFactory.Create();
        _repository = new CardRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_SavesCard()
    {
        var card = EntityFactory.CreateCard();

        await _repository.AddAsync(card);
        await _context.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(card.Id);
        Assert.NotNull(saved);
        Assert.Equal(card.CardNumber, saved.CardNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCardNumberAsync_ReturnsCard_WhenExists()
    {
        var card = EntityFactory.CreateCard();
        await _repository.AddAsync(card);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCardNumberAsync(card.CardNumber);

        Assert.NotNull(result);
        Assert.Equal(card.Id, result.Id);
    }

    [Fact]
    public async Task GetByCardNumberAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByCardNumberAsync("4000 0000 0000 0000");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsAllAccountCards()
    {
        var accountId = Guid.NewGuid();
        await _repository.AddAsync(EntityFactory.CreateCard(accountId, "HOLDER ONE", CardType.Debit));
        await _repository.AddAsync(EntityFactory.CreateCard(accountId, "HOLDER ONE", CardType.Credit));
        await _repository.AddAsync(EntityFactory.CreateCard(Guid.NewGuid()));
        await _context.SaveChangesAsync();

        var result = await _repository.GetByAccountIdAsync(accountId);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(accountId, c.AccountId));
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsEmpty_WhenNoCards()
    {
        var result = await _repository.GetByAccountIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Delete_RemovesCard()
    {
        var card = EntityFactory.CreateCard();
        await _repository.AddAsync(card);
        await _context.SaveChangesAsync();

        _repository.Delete(card);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(card.Id);
        Assert.Null(result);
    }
}
