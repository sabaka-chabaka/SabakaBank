using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class AccountsControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly SabakaWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AccountsControllerTests(SabakaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> CreateAuthenticatedClientAsync(string username, string email)
    {
        var client = _factory.CreateClient();
        var token  = await AuthHelper.RegisterAndLoginAsync(client, username, email);
        AuthHelper.SetBearer(client, token);
        return (client, token);
    }

    private static async Task<Guid> CreateAccountAsync(HttpClient client, string type = "Checking", string currency = "USD")
    {
        var resp = await client.PostAsJsonAsync("/api/accounts", new { Type = type, Currency = currency });
        var body = await resp.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateAccount_Returns201_WhenValid()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser1", "accuser1@bank.com");

        var resp = await client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_Returns401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetMyAccounts_Returns200_WithAccounts()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser2", "accuser2@bank.com");
        await CreateAccountAsync(client);
        await CreateAccountAsync(client, "Savings", "SC");

        var resp = await client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var accounts = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(accounts!);
    }

    [Fact]
    public async Task GetAccount_Returns200_WhenOwner()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser3", "accuser3@bank.com");
        var id = await CreateAccountAsync(client);

        var resp = await client.GetAsync($"/api/accounts/{id}");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAccount_Returns404_WhenNotFound()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser4", "accuser4@bank.com");

        var resp = await client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Deposit_Returns204_WhenValid()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser5", "accuser5@bank.com");
        var id = await CreateAccountAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 500m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Withdraw_Returns204_WhenSufficientFunds()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser6", "accuser6@bank.com");
        var id = await CreateAccountAsync(client);
        await client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 500m, Currency = "USD" });

        var resp = await client.PostAsJsonAsync($"/api/accounts/{id}/withdraw", new { Amount = 200m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Withdraw_Returns400_WhenInsufficientFunds()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser7", "accuser7@bank.com");
        var id = await CreateAccountAsync(client);

        var resp = await client.PostAsJsonAsync($"/api/accounts/{id}/withdraw", new { Amount = 999m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns204_WhenValid()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser8", "accuser8@bank.com");
        var fromId = await CreateAccountAsync(client);
        var toId   = await CreateAccountAsync(client);
        await client.PostAsJsonAsync($"/api/accounts/{fromId}/deposit", new { Amount = 500m, Currency = "USD" });

        var resp = await client.PostAsJsonAsync($"/api/accounts/{fromId}/transfer", new
        {
            ToAccountId = toId, Amount = 200m, Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_Returns200_WithHistory()
    {
        var (client, _) = await CreateAuthenticatedClientAsync("accuser9", "accuser9@bank.com");
        var id = await CreateAccountAsync(client);
        await client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 100m, Currency = "USD" });

        var resp = await client.GetAsync($"/api/accounts/{id}/transactions");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var txs = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(txs!);
    }
}