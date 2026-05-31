using System.Net.Http.Json;
using System.Text.Json;

namespace SabakaBank.Backend.API.UnitTests;

internal static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<string> RegisterAndLoginAsync(HttpClient client,
        string username = "sabaka",
        string email = "sabaka@bank.com",
        string password = "password123")
    {
        await client.PostAsJsonAsync("/api/users/register", new
        {
            Username  = username,
            Email     = email,
            Password  = password,
            FirstName = "Sabaka",
            LastName  = "Chabaka"
        });

        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email    = email,
            Password = password
        });

        var body  = await loginResp.Content.ReadAsStringAsync();
        var doc   = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    public static void SetBearer(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
}
