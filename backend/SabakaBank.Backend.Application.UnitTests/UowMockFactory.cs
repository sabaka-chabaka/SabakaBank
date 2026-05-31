using Moq;
using SabakaBank.Backend.Domain.Repositories;

namespace SabakaBank.Backend.Application.UnitTests;

internal static class UowMockFactory
{
    public static (Mock<IUnitOfWork> Uow,
                   Mock<IUserRepository> Users,
                   Mock<IAccountRepository> Accounts,
                   Mock<ICardRepository> Cards,
                   Mock<ITransactionRepository> Transactions) Create()
    {
        var users        = new Mock<IUserRepository>();
        var accounts     = new Mock<IAccountRepository>();
        var cards        = new Mock<ICardRepository>();
        var transactions = new Mock<ITransactionRepository>();
        var uow          = new Mock<IUnitOfWork>();

        uow.Setup(u => u.Users).Returns(users.Object);
        uow.Setup(u => u.Accounts).Returns(accounts.Object);
        uow.Setup(u => u.Cards).Returns(cards.Object);
        uow.Setup(u => u.Transactions).Returns(transactions.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (uow, users, accounts, cards, transactions);
    }
}
