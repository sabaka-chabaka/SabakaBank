using SabakaBank.Backend.Infrastructure.Persistence;
using SabakaBank.Backend.Infrastructure.Repositories;
using SabakaBank.Backend.Infrastructure.UnitTests;
using Xunit;

namespace SabakaBank.Backend.Infrastructure.UnitTests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _uow;

    public UnitOfWorkTests()
    {
        _context = DbContextFactory.Create();
        _uow     = new UnitOfWork(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void UnitOfWork_ExposesAllRepositories()
    {
        Assert.NotNull(_uow.Users);
        Assert.NotNull(_uow.Accounts);
        Assert.NotNull(_uow.Cards);
        Assert.NotNull(_uow.Transactions);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsAddedEntity()
    {
        var user = EntityFactory.CreateUser();

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Users.GetByIdAsync(user.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsMultipleEntitiesInOneTransaction()
    {
        var user    = EntityFactory.CreateUser();
        var account = EntityFactory.CreateAccount(user.Id);

        await _uow.Users.AddAsync(user);
        await _uow.Accounts.AddAsync(account);
        await _uow.SaveChangesAsync();

        var savedUser    = await _uow.Users.GetByIdAsync(user.Id);
        var savedAccount = await _uow.Accounts.GetByIdAsync(account.Id);

        Assert.NotNull(savedUser);
        Assert.NotNull(savedAccount);
        Assert.Equal(user.Id, savedAccount.UserId);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsDeletion()
    {
        var user = EntityFactory.CreateUser();
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        _uow.Users.Delete(user);
        await _uow.SaveChangesAsync();

        var result = await _uow.Users.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChangesAsync_ReturnsAffectedRowCount()
    {
        var user = EntityFactory.CreateUser();
        await _uow.Users.AddAsync(user);

        var count = await _uow.SaveChangesAsync();

        Assert.Equal(1, count);
    }
}
