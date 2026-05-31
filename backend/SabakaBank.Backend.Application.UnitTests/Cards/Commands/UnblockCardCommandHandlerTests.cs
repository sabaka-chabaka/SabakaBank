using Moq;
using SabakaBank.Backend.Application.Cards.Commands.BlockCard;
using SabakaBank.Backend.Application.Cards.Commands.UnblockCard;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using Xunit;

namespace SabakaBank.Backend.Application.UnitTests.Cards.Commands;

public class UnblockCardCommandHandlerTests
{
    private readonly UnblockCardCommandHandler _handler;
    private readonly BlockCardCommandHandler _blocker;
    private readonly Mock<ICardRepository> _cards;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUnitOfWork> _uow;

    public UnblockCardCommandHandlerTests()
    {
        var (uow, _, accounts, cards, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _cards    = cards;
        _handler  = new UnblockCardCommandHandler(_uow.Object);
        _blocker  = new BlockCardCommandHandler(_uow.Object);
    }

    private async Task<(Account account, Card card)> BlockedCard()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        await _blocker.Handle(new BlockCardCommand(card.Id, account.UserId), default);
        return (account, card);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var (account, card) = await BlockedCard();

        var error = await _handler.Handle(new UnblockCardCommand(card.Id, account.UserId), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_UnblocksCard()
    {
        var (account, card) = await BlockedCard();

        await _handler.Handle(new UnblockCardCommand(card.Id, account.UserId), default);

        Assert.Equal(Domain.Enums.CardStatus.Active, card.Status);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCardMissing()
    {
        _cards.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

        var error = await _handler.Handle(new UnblockCardCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var (_, card) = await BlockedCard();

        var error = await _handler.Handle(new UnblockCardCommand(card.Id, Guid.NewGuid()), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsInvalidOperation_WhenCardNotBlocked()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new UnblockCardCommand(card.Id, account.UserId), default);

        Assert.NotNull(error);
        Assert.Equal("InvalidOperation", error.Code);
    }
}
