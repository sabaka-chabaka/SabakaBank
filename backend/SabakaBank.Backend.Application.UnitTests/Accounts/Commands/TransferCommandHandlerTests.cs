using Moq;
using SabakaBank.Backend.Application.Accounts.Commands.Deposit;
using SabakaBank.Backend.Application.Accounts.Commands.Transfer;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Domain.Repositories;
using Xunit;

namespace SabakaBank.Backend.Application.UnitTests.Accounts.Commands;

public class TransferCommandHandlerTests
{
    private readonly TransferCommandHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUnitOfWork> _uow;

    public TransferCommandHandlerTests()
    {
        var (uow, _, accounts, _, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _handler  = new TransferCommandHandler(_uow.Object);
    }

    private async Task<Account> AccountWithBalance(decimal balance)
    {
        var account = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        var deposit = new DepositCommandHandler(_uow.Object);
        await deposit.Handle(new DepositCommand(account.Id, account.UserId, balance, Currency.USD), default);
        return account;
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var from = await AccountWithBalance(500m);
        var to   = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var error = await _handler.Handle(new TransferCommand(from.Id, to.Id, from.UserId, 200m, Currency.USD), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_MovesBalance()
    {
        var from = await AccountWithBalance(500m);
        var to   = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        await _handler.Handle(new TransferCommand(from.Id, to.Id, from.UserId, 200m, Currency.USD), default);

        Assert.Equal(300m, from.Balance);
        Assert.Equal(200m, to.Balance);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenSameAccount()
    {
        var account = await AccountWithBalance(500m);

        var error = await _handler.Handle(new TransferCommand(account.Id, account.Id, account.UserId, 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("InvalidOperation", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenFromMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var error = await _handler.Handle(new TransferCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var from = await AccountWithBalance(500m);
        var to   = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var error = await _handler.Handle(new TransferCommand(from.Id, to.Id, Guid.NewGuid(), 100m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenInsufficientFunds()
    {
        var from = await AccountWithBalance(50m);
        var to   = EntityFactory.CreateAccount(currency: Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(to.Id, It.IsAny<CancellationToken>())).ReturnsAsync(to);

        var error = await _handler.Handle(new TransferCommand(from.Id, to.Id, from.UserId, 500m, Currency.USD), default);

        Assert.NotNull(error);
        Assert.Equal("InvalidOperation", error.Code);
    }
}
