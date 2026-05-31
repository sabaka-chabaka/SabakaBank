using Moq;
using SabakaBank.Backend.Application.Accounts.Commands.Deposit;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.UnitTests.Accounts.Commands;

public class DepositCommandHandlerTests
{
    private readonly DepositCommandHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUnitOfWork> _uow;

    public DepositCommandHandlerTests()
    {
        var (uow, _, accounts, _, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _handler  = new DepositCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new DepositCommand(account.Id, account.UserId, 100m, Currency.USD), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_IncreasesBalance()
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new DepositCommand(account.Id, account.UserId, 250m, Currency.USD), default);

        Assert.Equal(250m, account.Balance);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAccountMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var error = await _handler.Handle(new DepositCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new DepositCommand(account.Id, Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenCurrencyMismatch()
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new DepositCommand(account.Id, account.UserId, 100m, Currency.SC), default);

        Assert.NotNull(error);
        Assert.Equal("InvalidOperation", error.Code);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new DepositCommand(account.Id, account.UserId, 100m, Currency.USD), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
