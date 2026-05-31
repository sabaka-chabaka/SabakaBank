using System.Net;
using System.Net.Http.Json;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class UsersControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(SabakaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Returns201_WhenValid()
    {
        var resp = await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "newuser", Email = "new@bank.com", Password = "pass123",
            FirstName = "New", LastName = "User"
        });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Register_Returns409_WhenEmailTaken()
    {
        await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "first", Email = "taken@bank.com", Password = "pass",
            FirstName = "A", LastName = "B"
        });

        var resp = await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "second", Email = "taken@bank.com", Password = "pass",
            FirstName = "C", LastName = "D"
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Register_Returns409_WhenUsernameTaken()
    {
        await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "sameuser", Email = "email1@bank.com", Password = "pass",
            FirstName = "A", LastName = "B"
        });

        var resp = await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "sameuser", Email = "email2@bank.com", Password = "pass",
            FirstName = "C", LastName = "D"
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task GetMe_Returns200_WhenAuthenticated()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "meuser", "me@bank.com");
        AuthHelper.SetBearer(_client, token);

        var resp = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetMe_Returns401_WhenUnauthenticated()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        var resp = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns204_WhenValid()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "chpass", "chpass@bank.com", "oldpass");
        AuthHelper.SetBearer(_client, token);

        var resp = await _client.PutAsJsonAsync("/api/users/me/password", new
        {
            CurrentPassword = "oldpass",
            NewPassword     = "newpass"
        });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns403_WhenWrongCurrent()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client, "chpasswrong", "chpasswrong@bank.com", "correct");
        AuthHelper.SetBearer(_client, token);

        var resp = await _client.PutAsJsonAsync("/api/users/me/password", new
        {
            CurrentPassword = "wrong",
            NewPassword     = "new"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}
