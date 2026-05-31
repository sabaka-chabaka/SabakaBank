using Moq;
using SabakaBank.Backend.Application.Cards.Commands.IssueCard;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Domain.Repositories;
using Xunit;

namespace SabakaBank.Backend.Application.UnitTests.Cards.Commands;

public class IssueCardCommandHandlerTests
{
    private readonly IssueCardCommandHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUserRepository> _users;
    private readonly Mock<IUnitOfWork> _uow;

    public IssueCardCommandHandlerTests()
    {
        var (uow, users, accounts, _, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _users    = users;
        _handler  = new IssueCardCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCardId_WhenValid()
    {
        var user    = EntityFactory.CreateUser();
        var account = EntityFactory.CreateAccount(userId: user.Id);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new IssueCardCommand(account.Id, user.Id, CardType.Debit), default);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAccountMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await _handler.Handle(new IssueCardCommand(Guid.NewGuid(), Guid.NewGuid(), CardType.Debit), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new IssueCardCommand(account.Id, Guid.NewGuid(), CardType.Debit), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenAccountInactive()
    {
        var user    = EntityFactory.CreateUser();
        var account = EntityFactory.CreateAccount(userId: user.Id);
        account.Deactivate();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new IssueCardCommand(account.Id, user.Id, CardType.Debit), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidOperation", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        var user    = EntityFactory.CreateUser();
        var account = EntityFactory.CreateAccount(userId: user.Id);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _handler.Handle(new IssueCardCommand(account.Id, user.Id, CardType.Credit), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
