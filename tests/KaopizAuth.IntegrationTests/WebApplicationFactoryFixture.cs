using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace KaopizAuth.IntegrationTests;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;

    public WebApplicationFactoryFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("kaopizauth_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        _redisContainer = new RedisBuilder()
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing database configuration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Add test database configuration
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Configure Redis for testing
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _postgresContainer?.DisposeAsync().AsTask().Wait();
            _redisContainer?.DisposeAsync().AsTask().Wait();
        }
        base.Dispose(disposing);
    }
}