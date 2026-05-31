using Moq;
using SabakaBank.Backend.Application.Accounts.Commands.Deposit;
using SabakaBank.Backend.Application.Accounts.Commands.Withdraw;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.UnitTests.Accounts.Commands;

public class WithdrawCommandHandlerTests
{
    private readonly WithdrawCommandHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUnitOfWork> _uow;

    public WithdrawCommandHandlerTests()
    {
        var (uow, _, accounts, _, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _handler  = new WithdrawCommandHandler(_uow.Object);
    }

    private async Task<Account> AccountWithBalance(decimal balance)
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        var depositHandler = new DepositCommandHandler(_uow.Object);
        await depositHandler.Handle(new DepositCommand(account.Id, account.UserId, balance, Currency.USD), default);
        return account;
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var account = await AccountWithBalance(500m);

        var error = await _handler.Handle(new WithdrawCommand(account.Id, account.UserId, 200m, Currency.USD), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_DecreasesBalance()
    {
        var account = await AccountWithBalance(500m);

        await _handler.Handle(new WithdrawCommand(account.Id, account.UserId, 200m, Currency.USD), default);

        Assert.Equal(300m, account.Balance);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenInsufficientFunds()
    {
        var account = await AccountWithBalance(50m);

        var error = await _handler.Handle(new WithdrawCommand(account.Id, account.UserId, 200m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("InvalidOperation", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAccountMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var error = await _handler.Handle(new WithdrawCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = await AccountWithBalance(500m);

        var error = await _handler.Handle(new WithdrawCommand(account.Id, Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }
}
