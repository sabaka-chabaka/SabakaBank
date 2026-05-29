using SabakaBank.Backend.Infrastructure.Persistence;
using SabakaBank.Backend.Infrastructure.Repositories;
using SabakaBank.Backend.Infrastructure.UnitTests;
using Xunit;

namespace SabakaBank.Backend.Infrastructure.UnitTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _context    = DbContextFactory.Create();
        _repository = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_SavesUser()
    {
        var user = EntityFactory.CreateUser();

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(saved);
        Assert.Equal(user.Username, saved.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var user = EntityFactory.CreateUser(email: "unique@test.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("unique@test.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByEmailAsync("nobody@test.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUser_WhenExists()
    {
        var user = EntityFactory.CreateUser(username: "sabaka");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUsernameAsync("sabaka");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByUsernameAsync("ghost");

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsTrue_WhenExists()
    {
        var user = EntityFactory.CreateUser(email: "exists@test.com");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByEmailAsync("exists@test.com");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsFalse_WhenNotFound()
    {
        var result = await _repository.ExistsByEmailAsync("nope@test.com");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByUsernameAsync_ReturnsTrue_WhenExists()
    {
        var user = EntityFactory.CreateUser(username: "existinguser");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByUsernameAsync("existinguser");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByUsernameAsync_ReturnsFalse_WhenNotFound()
    {
        var result = await _repository.ExistsByUsernameAsync("nobody");

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await _repository.AddAsync(EntityFactory.CreateUser(username: "u1", email: "u1@test.com"));
        await _repository.AddAsync(EntityFactory.CreateUser(username: "u2", email: "u2@test.com"));
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Delete_RemovesUser()
    {
        var user = EntityFactory.CreateUser();
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        _repository.Delete(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Null(result);
    }
}
