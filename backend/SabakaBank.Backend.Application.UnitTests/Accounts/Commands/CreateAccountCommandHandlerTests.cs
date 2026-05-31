using Moq;
using SabakaBank.Backend.Application.Accounts.Commands.CreateAccount;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.UnitTests.Accounts.Commands;

public class CreateAccountCommandHandlerTests
{
    private readonly CreateAccountCommandHandler _handler;
    private readonly Mock<IUserRepository> _users;
    private readonly Mock<IUnitOfWork> _uow;

    public CreateAccountCommandHandlerTests()
    {
        var (uow, users, _, _, _) = UowMockFactory.Create();
        _uow     = uow;
        _users   = users;
        _handler = new CreateAccountCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAccountId_WhenValid()
    {
        var user = EntityFactory.CreateUser();
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new CreateAccountCommand(user.Id, AccountType.Checking, Currency.USD), default);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        _users.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new CreateAccountCommand(Guid.NewGuid(), AccountType.Checking, Currency.USD), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenUserDeactivated()
    {
        var user = EntityFactory.CreateUser(active: false);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new CreateAccountCommand(user.Id, AccountType.Checking, Currency.USD), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        var user = EntityFactory.CreateUser();
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _handler.Handle(new CreateAccountCommand(user.Id, AccountType.Savings, Currency.SC), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
