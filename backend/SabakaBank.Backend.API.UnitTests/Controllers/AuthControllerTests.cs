using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class AuthControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly SabakaWebApplicationFactory _factory;

    public AuthControllerTests(SabakaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_Returns200_WithToken_WhenValid()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "loginuser", Email = "login@bank.com", Password = "pass123",
            FirstName = "Login", LastName = "User"
        });

        var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "login@bank.com", Password = "pass123"
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(body!.Token);
        Assert.NotEqual(Guid.Empty, body.UserId);
    }

    [Fact]
    public async Task Login_Returns403_WhenWrongPassword()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/users/register", new
        {
            Username = "wrongpassuser", Email = "wrongpass@bank.com", Password = "correct",
            FirstName = "A", LastName = "B"
        });

        var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrongpass@bank.com", Password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Login_Returns403_WhenUserNotFound()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "ghost@bank.com", Password = "pass"
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    private record LoginResponse(Guid UserId, string Token);
}