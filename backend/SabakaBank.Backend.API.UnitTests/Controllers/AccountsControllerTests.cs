using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class AccountsControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AccountsControllerTests(SabakaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync(string username, string email)
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client, username, email);
        AuthHelper.SetBearer(_client, token);
    }

    private async Task<Guid> CreateAccountAsync(string type = "Checking", string currency = "USD")
    {
        var resp = await _client.PostAsJsonAsync("/api/accounts", new { Type = type, Currency = currency });
        var body = await resp.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateAccount_Returns201_WhenValid()
    {
        await AuthenticateAsync("accuser1", "accuser1@bank.com");

        var resp = await _client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_Returns401_WhenUnauthenticated()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        var resp = await client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetMyAccounts_Returns200_WithAccounts()
    {
        await AuthenticateAsync("accuser2", "accuser2@bank.com");
        await CreateAccountAsync();
        await CreateAccountAsync("Savings", "SC");

        var resp = await _client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var accounts = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(accounts!);
    }

    [Fact]
    public async Task GetAccount_Returns200_WhenOwner()
    {
        await AuthenticateAsync("accuser3", "accuser3@bank.com");
        var id = await CreateAccountAsync();

        var resp = await _client.GetAsync($"/api/accounts/{id}");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAccount_Returns404_WhenNotFound()
    {
        await AuthenticateAsync("accuser4", "accuser4@bank.com");

        var resp = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Deposit_Returns204_WhenValid()
    {
        await AuthenticateAsync("accuser5", "accuser5@bank.com");
        var id = await CreateAccountAsync();

        var resp = await _client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 500m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Withdraw_Returns204_WhenSufficientFunds()
    {
        await AuthenticateAsync("accuser6", "accuser6@bank.com");
        var id = await CreateAccountAsync();
        await _client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 500m, Currency = "USD" });

        var resp = await _client.PostAsJsonAsync($"/api/accounts/{id}/withdraw", new { Amount = 200m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Withdraw_Returns400_WhenInsufficientFunds()
    {
        await AuthenticateAsync("accuser7", "accuser7@bank.com");
        var id = await CreateAccountAsync();

        var resp = await _client.PostAsJsonAsync($"/api/accounts/{id}/withdraw", new { Amount = 999m, Currency = "USD" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns204_WhenValid()
    {
        await AuthenticateAsync("accuser8", "accuser8@bank.com");
        var fromId = await CreateAccountAsync();
        var toId   = await CreateAccountAsync();
        await _client.PostAsJsonAsync($"/api/accounts/{fromId}/deposit", new { Amount = 500m, Currency = "USD" });

        var resp = await _client.PostAsJsonAsync($"/api/accounts/{fromId}/transfer", new
        {
            ToAccountId = toId, Amount = 200m, Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_Returns200_WithHistory()
    {
        await AuthenticateAsync("accuser9", "accuser9@bank.com");
        var id = await CreateAccountAsync();
        await _client.PostAsJsonAsync($"/api/accounts/{id}/deposit", new { Amount = 100m, Currency = "USD" });

        var resp = await _client.GetAsync($"/api/accounts/{id}/transactions");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var txs = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(txs!);
    }
}
