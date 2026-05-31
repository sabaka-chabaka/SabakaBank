using Moq;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Application.Users.Commands.Register;

namespace SabakaBank.Backend.Application.UnitTests.Users.Commands;

public class RegisterCommandHandlerTests
{
    private readonly RegisterCommandHandler _handler;
    private readonly Mock<IUserRepository> _users;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IPasswordService> _passwords;

    public RegisterCommandHandlerTests()
    {
        var (uow, users, _, _, _) = UowMockFactory.Create();
        _uow       = uow;
        _users     = users;
        _passwords = new Mock<IPasswordService>();
        _passwords.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed");
        _handler   = new RegisterCommandHandler(_uow.Object, _passwords.Object);
    }

    private static RegisterCommand ValidCommand() =>
        new("sabaka", "sabaka@bank.com", "password123", "Sabaka", "Chabaka");

    [Fact]
    public async Task Handle_ReturnsUserId_WhenValid()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(ValidCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenEmailTaken()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(ValidCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenUsernameTaken()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(ValidCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_HashesPassword()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _handler.Handle(ValidCommand(), default);

        _passwords.Verify(p => p.Hash("password123"), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenValid()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(u => u.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _handler.Handle(ValidCommand(), default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
