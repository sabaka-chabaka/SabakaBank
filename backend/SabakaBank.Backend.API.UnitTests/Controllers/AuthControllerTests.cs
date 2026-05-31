using System.Net;
using System.Net.Http.Json;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class AuthControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(SabakaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Returns200_WithToken_WhenValid()
    {
        await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "loginuser", Email = "login@bank.com", Password = "pass123",
            FirstName = "Login", LastName = "User"
        });

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "login@bank.com", Password = "pass123"
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body!.Token);
        Assert.NotEqual(Guid.Empty, body.UserId);
    }

    [Fact]
    public async Task Login_Returns403_WhenWrongPassword()
    {
        await _client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "wrongpassuser", Email = "wrongpass@bank.com", Password = "correct",
            FirstName = "A", LastName = "B"
        });

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrongpass@bank.com", Password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Login_Returns403_WhenUserNotFound()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "ghost@bank.com", Password = "pass"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    private record LoginResponse(Guid UserId, string Token);
}
