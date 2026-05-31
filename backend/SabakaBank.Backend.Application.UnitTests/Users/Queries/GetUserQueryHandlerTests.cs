using Moq;
using SabakaBank.Backend.Application.UnitTests;
using SabakaBank.Backend.Application.Users.Queries.GetUser;

namespace SabakaBank.Backend.Application.UnitTests.Users.Queries;

public class GetUserQueryHandlerTests
{
    private readonly GetUserQueryHandler _handler;
    private readonly Mock<IUserRepository> _users;

    public GetUserQueryHandlerTests()
    {
        var (uow, users, _, _, _) = UowMockFactory.Create();
        _users   = users;
        _handler = new GetUserQueryHandler(uow.Object);
    }

    [Fact]
    public async Task Handle_ReturnsUserDto_WhenFound()
    {
        var user = EntityFactory.CreateUser();
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _handler.Handle(new GetUserQuery(user.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value!.Id);
        Assert.Equal(user.Username, result.Value.Username);
        Assert.Equal(user.Email, result.Value.Email);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenMissing()
    {
        _users.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new GetUserQuery(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }
}
