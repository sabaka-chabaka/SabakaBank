using System.Security.Claims;

namespace SabakaBank.Backend.API.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub");

        return Guid.Parse(sub!);
    }
}
