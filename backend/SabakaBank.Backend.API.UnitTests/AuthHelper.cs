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
        var registerResp = await client.PostAsJsonAsync("/api/users/register", new
        {
            Username  = username,
            Email     = email,
            Password  = password,
            FirstName = "Sabaka",
            LastName  = "Chabaka"
        });

        if (!registerResp.IsSuccessStatusCode)
        {
            var err = await registerResp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed ({registerResp.StatusCode}): {err}");
        }

        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email    = email,
            Password = password
        });

        if (!loginResp.IsSuccessStatusCode)
        {
            var err = await loginResp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed ({loginResp.StatusCode}): {err}");
        }

        var body   = await loginResp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResult>(body, JsonOptions)
                     ?? throw new InvalidOperationException($"Login response deserialization failed. Body: {body}");

        return result.Token;
    }

    public static void SetBearer(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    private record LoginResult(Guid UserId, string Token);
}