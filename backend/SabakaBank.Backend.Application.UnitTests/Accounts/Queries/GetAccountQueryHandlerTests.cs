using Moq;
using SabakaBank.Backend.Application.Accounts.Queries.GetAccount;
using SabakaBank.Backend.Application.UnitTests;

namespace SabakaBank.Backend.Application.UnitTests.Accounts.Queries;

public class GetAccountQueryHandlerTests
{
    private readonly GetAccountQueryHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;

    public GetAccountQueryHandlerTests()
    {
        var (uow, _, accounts, _, _) = UowMockFactory.Create();
        _accounts = accounts;
        _handler  = new GetAccountQueryHandler(uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAccountDto_WhenOwner()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new GetAccountQuery(account.Id, account.UserId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(account.Id, result.Value!.Id);
        Assert.Equal(account.AccountNumber, result.Value.AccountNumber);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await _handler.Handle(new GetAccountQuery(Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new GetAccountQuery(account.Id, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }
}
