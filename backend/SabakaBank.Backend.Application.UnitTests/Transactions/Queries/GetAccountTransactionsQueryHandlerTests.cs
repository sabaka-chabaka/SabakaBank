using Moq;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Application.Transactions.Queries.GetAccountTransactions;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.Application.UnitTests.Transactions.Queries;

public class GetAccountTransactionsQueryHandlerTests
{
    private readonly GetAccountTransactionsQueryHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<ITransactionRepository> _transactions;

    public GetAccountTransactionsQueryHandlerTests()
    {
        var (uow, _, accounts, _, transactions) = UowMockFactory.Create();
        _accounts     = accounts;
        _transactions = transactions;
        _handler      = new GetAccountTransactionsQueryHandler(uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTransactions_WhenOwner()
    {
        var account = EntityFactory.CreateAccount();
        var tx1     = Transaction.CreateDeposit(account.Id, 100m, Currency.USD);
        var tx2     = Transaction.CreateDeposit(account.Id, 200m, Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _transactions.Setup(t => t.GetByAccountIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([tx1, tx2]);

        var result = await _handler.Handle(new GetAccountTransactionsQuery(account.Id, account.UserId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Handle_MapsDtoCorrectly()
    {
        var account = EntityFactory.CreateAccount();
        var tx      = Transaction.CreateDeposit(account.Id, 100m, Currency.USD);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _transactions.Setup(t => t.GetByAccountIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([tx]);

        var result = await _handler.Handle(new GetAccountTransactionsQuery(account.Id, account.UserId), default);

        var dto = result.Value![0];
        Assert.Equal(tx.Id, dto.Id);
        Assert.Equal("Deposit", dto.Type);
        Assert.Equal("Pending", dto.Status);
        Assert.Equal(100m, dto.Amount);
        Assert.Equal("USD", dto.Currency);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAccountMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await _handler.Handle(new GetAccountTransactionsQuery(Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new GetAccountTransactionsQuery(account.Id, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoTransactions()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _transactions.Setup(t => t.GetByAccountIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new GetAccountTransactionsQuery(account.Id, account.UserId), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
