using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class CardsControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CardsControllerTests(SabakaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync(string username, string email)
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client, username, email);
        AuthHelper.SetBearer(_client, token);
    }

    private async Task<Guid> CreateAccountAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });
        var doc  = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> IssueCardAsync(Guid accountId, string type = "Debit")
    {
        var resp = await _client.PostAsJsonAsync($"/api/accounts/{accountId}/cards", new { Type = type });
        var doc  = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task IssueCard_Returns201_WhenValid()
    {
        await AuthenticateAsync("carduser1", "carduser1@bank.com");
        var accountId = await CreateAccountAsync();

        var resp = await _client.PostAsJsonAsync($"/api/accounts/{accountId}/cards", new { Type = "Debit" });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task GetCards_Returns200_WithCards()
    {
        await AuthenticateAsync("carduser2", "carduser2@bank.com");
        var accountId = await CreateAccountAsync();
        await IssueCardAsync(accountId);

        var resp = await _client.GetAsync($"/api/accounts/{accountId}/cards");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var cards = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(cards!);
    }

    [Fact]
    public async Task GetCards_ReturnsMaskedNumbers()
    {
        await AuthenticateAsync("carduser3", "carduser3@bank.com");
        var accountId = await CreateAccountAsync();
        await IssueCardAsync(accountId);

        var resp = await _client.GetAsync($"/api/accounts/{accountId}/cards");
        var body = await resp.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        var maskedNumber = doc.RootElement[0].GetProperty("maskedNumber").GetString();

        Assert.StartsWith("**** **** ****", maskedNumber);
    }

    [Fact]
    public async Task BlockCard_Returns204_WhenValid()
    {
        await AuthenticateAsync("carduser4", "carduser4@bank.com");
        var accountId = await CreateAccountAsync();
        var cardId    = await IssueCardAsync(accountId);

        var resp = await _client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/block", null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task UnblockCard_Returns204_AfterBlock()
    {
        await AuthenticateAsync("carduser5", "carduser5@bank.com");
        var accountId = await CreateAccountAsync();
        var cardId    = await IssueCardAsync(accountId);
        await _client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/block", null);

        var resp = await _client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/unblock", null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task BlockCard_Returns401_WhenUnauthenticated()
    {
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        var resp = await client.PostAsync($"/api/accounts/{Guid.NewGuid()}/cards/{Guid.NewGuid()}/block", null);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
