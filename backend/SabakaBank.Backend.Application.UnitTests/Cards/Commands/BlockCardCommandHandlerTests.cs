using Moq;
using SabakaBank.Backend.Application.Cards.Commands.BlockCard;
using SabakaBank.Backend.Application.UnitTests;

namespace SabakaBank.Backend.Application.UnitTests.Cards.Commands;

public class BlockCardCommandHandlerTests
{
    private readonly BlockCardCommandHandler _handler;
    private readonly Mock<ICardRepository> _cards;
    private readonly Mock<IAccountRepository> _accounts;
    private readonly Mock<IUnitOfWork> _uow;

    public BlockCardCommandHandlerTests()
    {
        var (uow, _, accounts, cards, _) = UowMockFactory.Create();
        _uow      = uow;
        _accounts = accounts;
        _cards    = cards;
        _handler  = new BlockCardCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new BlockCardCommand(card.Id, account.UserId), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_BlocksCard()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new BlockCardCommand(card.Id, account.UserId), default);

        Assert.Equal(Domain.Enums.CardStatus.Blocked, card.Status);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCardMissing()
    {
        _cards.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Card?)null);

        var error = await _handler.Handle(new BlockCardCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var error = await _handler.Handle(new BlockCardCommand(card.Id, Guid.NewGuid()), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        var account = EntityFactory.CreateAccount();
        var card    = EntityFactory.CreateCard(account.Id);
        _cards.Setup(c => c.GetByIdAsync(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        _accounts.Setup(a => a.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await _handler.Handle(new BlockCardCommand(card.Id, account.UserId), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
