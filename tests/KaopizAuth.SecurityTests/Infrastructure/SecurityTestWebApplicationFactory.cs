using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Infrastructure.Data;

namespace KaopizAuth.SecurityTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for security testing with in-memory database
/// </summary>
public class SecurityTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing database registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("SecurityTestDatabase_" + Guid.NewGuid());
            });
        });

        builder.UseEnvironment("Testing");
        
        // Configure test-specific settings
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("JWT:Issuer", "TestIssuer"),
                new KeyValuePair<string, string?>("JWT:Audience", "TestAudience"),
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "InMemory")
            });
        });
    }

    protected override void ConfigureWebHostDefaults(IWebHostBuilder builder)
    {
        base.ConfigureWebHostDefaults(builder);
        
        // Ensure we use the test environment
        builder.UseEnvironment("Testing");
    }

    public async Task SeedTestDataAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed test data
        await SeedTestData(context, userManager);
    }

    private static async Task SeedTestData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Check if users already exist
        if (await userManager.FindByEmailAsync("securitytest@example.com") != null)
        {
            return; // Already seeded
        }

        // Create test user for security testing
        var testUser = new ApplicationUser
        {
            UserName = "securitytest@example.com",
            Email = "securitytest@example.com",
            FirstName = "Security",
            LastName = "Test",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(testUser, "SecurePassword123!");
        if (result.Succeeded)
        {
            await context.SaveChangesAsync();
        }

        // Create locked test user
        var lockedUser = new ApplicationUser
        {
            UserName = "locked@example.com",
            Email = "locked@example.com",
            FirstName = "Locked",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };

        await userManager.CreateAsync(lockedUser, "SecurePassword123!");
        await context.SaveChangesAsync();
    }
}