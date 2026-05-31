using Moq;
using SabakaBank.Backend.Application.Auth.Commands.Login;
using SabakaBank.Backend.Application.Common.Interfaces;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Domain.Entities;
using SabakaBank.Backend.Domain.Repositories;
using Xunit;

namespace SabakaBank.Backend.Application.UnitTests.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly LoginCommandHandler _handler;
    private readonly Mock<IUserRepository> _users;
    private readonly Mock<IPasswordService> _passwords;
    private readonly Mock<IJwtService> _jwt;

    public LoginCommandHandlerTests()
    {
        var (uow, users, _, _, _) = UowMockFactory.Create();
        _users     = users;
        _passwords = new Mock<IPasswordService>();
        _jwt       = new Mock<IJwtService>();
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");
        _handler   = new LoginCommandHandler(uow.Object, _passwords.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ReturnsToken_WhenCredentialsValid()
    {
        var user = EntityFactory.CreateUser(passwordHash: "hash");
        _users.Setup(u => u.GetByEmailAsync("sabaka@bank.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("pass", "hash")).Returns(true);

        var result = await _handler.Handle(new LoginCommand("sabaka@bank.com", "pass"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value!.Token);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenUserNotFound()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new LoginCommand("nobody@bank.com", "pass"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenPasswordWrong()
    {
        var user = EntityFactory.CreateUser(passwordHash: "hash");
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("wrong", "hash")).Returns(false);

        var result = await _handler.Handle(new LoginCommand("sabaka@bank.com", "wrong"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenUserDeactivated()
    {
        var user = EntityFactory.CreateUser(passwordHash: "hash", active: false);
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwords.Setup(p => p.Verify("pass", "hash")).Returns(true);

        var result = await _handler.Handle(new LoginCommand("sabaka@bank.com", "pass"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }
}
