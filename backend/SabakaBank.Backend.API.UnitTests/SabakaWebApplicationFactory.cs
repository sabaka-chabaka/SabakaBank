using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SabakaBank.Backend.Infrastructure.Persistence;

namespace SabakaBank.Backend.API.UnitTests;

public class SabakaWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]        = "test-secret-key-32chars-long-here",
                ["Jwt:Issuer"]        = "SabakaBank",
                ["Jwt:Audience"]      = "SabakaBank",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Cors:Origin"]       = "http://localhost:5173"
            });
        });
    }
}
