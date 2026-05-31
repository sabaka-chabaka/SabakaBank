using Moq;
using SabakaBank.Backend.Application.Cards.Queries.GetAccountCards;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Enums;
using SabakaBank.Backend.Domain.Repositories;
using Xunit;

namespace SabakaBank.Backend.Application.UnitTests.Cards.Queries;

public class GetAccountCardsQueryHandlerTests
{
    private readonly GetAccountCardsQueryHandler _handler;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<ICardRepository> _cards;

    public GetAccountCardsQueryHandlerTests()
    {
        var (uow, _, accounts, cards, _) = UowMockFactory.Create();
        _accounts = accounts;
        _cards    = cards;
        _handler  = new GetAccountCardsQueryHandler(uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCards_WhenOwner()
    {
        var account = EntityFactory.CreateAccount();
        var card1   = EntityFactory.CreateCard(account.Id, CardType.Debit);
        var card2   = EntityFactory.CreateCard(account.Id, CardType.Credit);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _cards.Setup(c => c.GetByAccountIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([card1, card2]);

        var result = await _handler.Handle(new GetAccountCardsQuery(account.Id, account.UserId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Handle_ReturnsMaskedNumber()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _cards.Setup(c => c.GetByAccountIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([card]);

        var result = await _handler.Handle(new GetAccountCardsQuery(account.Id, account.UserId), default);

        Assert.StartsWith("**** **** ****", result.Value![0].MaskedNumber);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenAccountMissing()
    {
        _accounts.Setup(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var result = await _handler.Handle(new GetAccountCardsQuery(Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount();
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await _handler.Handle(new GetAccountCardsQuery(account.Id, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }
}
