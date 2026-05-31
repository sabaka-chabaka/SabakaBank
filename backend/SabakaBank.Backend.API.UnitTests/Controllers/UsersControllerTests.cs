using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class UsersControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly SabakaWebApplicationFactory _factory;

    public UsersControllerTests(SabakaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Returns201_WhenValid()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "newuser", Email = "new@bank.com", Password = "pass123",
            FirstName = "New", LastName = "User"
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Register_Returns409_WhenEmailTaken()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "first", Email = "taken@bank.com", Password = "pass",
            FirstName = "A", LastName = "B"
        });

        var resp = await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "second", Email = "taken@bank.com", Password = "pass",
            FirstName = "C", LastName = "D"
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Register_Returns409_WhenUsernameTaken()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "sameuser", Email = "email1@bank.com", Password = "pass",
            FirstName = "A", LastName = "B"
        });

        var resp = await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "sameuser", Email = "email2@bank.com", Password = "pass",
            FirstName = "C", LastName = "D"
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task GetMe_Returns200_WhenAuthenticated()
    {
        var client = _factory.CreateClient();
        var token  = await AuthHelper.RegisterAndLoginAsync(client, "meuser", "me@bank.com");
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMe_Returns401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns204_WhenValid()
    {
        var client = _factory.CreateClient();
        var token  = await AuthHelper.RegisterAndLoginAsync(client, "chpass", "chpass@bank.com", "oldpass");
        AuthHelper.SetBearer(client, token);

        var resp = await client.PutAsJsonAsync("/api/users/me/password", new
        {
            CurrentPassword = "oldpass",
            NewPassword     = "newpass"
        });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns403_WhenWrongCurrent()
    {
        var client = _factory.CreateClient();
        var token  = await AuthHelper.RegisterAndLoginAsync(client, "chpasswrong", "chpasswrong@bank.com", "correct");
        AuthHelper.SetBearer(client, token);

        var resp = await client.PutAsJsonAsync("/api/users/me/password", new
        {
            CurrentPassword = "wrong",
            NewPassword     = "new"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}