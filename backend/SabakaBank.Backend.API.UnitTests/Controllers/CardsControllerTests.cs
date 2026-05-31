using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SabakaBank.Backend.API.UnitTests.Controllers;

public class CardsControllerTests : IClassFixture<SabakaWebApplicationFactory>
{
    private readonly SabakaWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CardsControllerTests(SabakaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, Guid accountId)> SetupAsync(string username, string email)
    {
        var client = _factory.CreateClient();
        var token  = await AuthHelper.RegisterAndLoginAsync(client, username, email);
        AuthHelper.SetBearer(client, token);

        var resp = await client.PostAsJsonAsync("/api/accounts", new { Type = "Checking", Currency = "USD" });
        var doc  = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var id   = doc.RootElement.GetProperty("id").GetGuid();

        return (client, id);
    }

    private static async Task<Guid> IssueCardAsync(HttpClient client, Guid accountId, string type = "Debit")
    {
        var resp = await client.PostAsJsonAsync($"/api/accounts/{accountId}/cards", new { Type = type });
        var doc  = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task IssueCard_Returns201_WhenValid()
    {
        var (client, accountId) = await SetupAsync("carduser1", "carduser1@bank.com");

        var resp = await client.PostAsJsonAsync($"/api/accounts/{accountId}/cards", new { Type = "Debit" });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task GetCards_Returns200_WithCards()
    {
        var (client, accountId) = await SetupAsync("carduser2", "carduser2@bank.com");
        await IssueCardAsync(client, accountId);

        var resp = await client.GetAsync($"/api/accounts/{accountId}/cards");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var cards = await resp.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotEmpty(cards!);
    }

    [Fact]
    public async Task GetCards_ReturnsMaskedNumbers()
    {
        var (client, accountId) = await SetupAsync("carduser3", "carduser3@bank.com");
        await IssueCardAsync(client, accountId);

        var resp = await client.GetAsync($"/api/accounts/{accountId}/cards");
        var body = await resp.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        var maskedNumber = doc.RootElement[0].GetProperty("maskedNumber").GetString();

        Assert.StartsWith("**** **** ****", maskedNumber);
    }

    [Fact]
    public async Task BlockCard_Returns204_WhenValid()
    {
        var (client, accountId) = await SetupAsync("carduser4", "carduser4@bank.com");
        var cardId = await IssueCardAsync(client, accountId);

        var resp = await client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/block", null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task UnblockCard_Returns204_AfterBlock()
    {
        var (client, accountId) = await SetupAsync("carduser5", "carduser5@bank.com");
        var cardId = await IssueCardAsync(client, accountId);
        await client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/block", null);

        var resp = await client.PostAsync($"/api/accounts/{accountId}/cards/{cardId}/unblock", null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task BlockCard_Returns401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsync($"/api/accounts/{Guid.NewGuid()}/cards/{Guid.NewGuid()}/block", null);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}