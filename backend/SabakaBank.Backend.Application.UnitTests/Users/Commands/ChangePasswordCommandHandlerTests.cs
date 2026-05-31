using Moq;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Application.Users.Commands.ChangePassword;

namespace SabakaBank.Backend.Application.UnitTests.Users.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly ChangePasswordCommandHandler _handler;
    private readonly Mock<IUserRepository> _users;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IPasswordService> _passwords;

    public ChangePasswordCommandHandlerTests()
    {
        var (uow, users, _, _, _) = UowMockFactory.Create();
        _uow       = uow;
        _users     = users;
        _passwords = new Mock<IPasswordService>();
        _handler   = new ChangePasswordCommandHandler(_uow.Object, _passwords.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenValid()
    {
        var user = EntityFactory.CreateUser(passwordHash: "oldhash");
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("oldpass", "oldhash")).Returns(true);
        _passwords.Setup(p => p.Hash("newpass")).Returns("newhash");

        var error = await _handler.Handle(new ChangePasswordCommand(user.Id, "oldpass", "newpass"), default);

        Assert.Null(error);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        _users.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var error = await _handler.Handle(new ChangePasswordCommand(Guid.NewGuid(), "old", "new"), default);

        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenWrongPassword()
    {
        var user = EntityFactory.CreateUser(passwordHash: "oldhash");
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("wrong", "oldhash")).Returns(false);

        var error = await _handler.Handle(new ChangePasswordCommand(user.Id, "wrong", "new"), default);

        Assert.NotNull(error);
        Assert.Equal("Forbidden", error.Code);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        var user = EntityFactory.CreateUser(passwordHash: "oldhash");
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("oldpass", "oldhash")).Returns(true);
        _passwords.Setup(p => p.Hash("newpass")).Returns("newhash");

        await _handler.Handle(new ChangePasswordCommand(user.Id, "oldpass", "newpass"), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
