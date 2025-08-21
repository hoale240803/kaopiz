using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KaopizAuth.Infrastructure.Services.Background;

/// <summary>
/// Background service for cleaning up expired refresh tokens
/// </summary>
public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token cleanup");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait 30 minutes before retrying
            }
        }
    }

    private async Task CleanupExpiredTokensAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        var refreshTokenDomainService = scope.ServiceProvider.GetRequiredService<IRefreshTokenDomainService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Get expired tokens
            var expiredTokens = await refreshTokenRepository.GetExpiredTokensAsync();
            
            if (expiredTokens.Any())
            {
                // Clean up expired tokens
                var cleanedCount = refreshTokenDomainService.CleanupExpiredTokens(expiredTokens, "TokenCleanupService");
                
                // Remove very old tokens (expired for more than 30 days)
                var removedCount = await refreshTokenRepository.RemoveExpiredTokensAsync();
                
                // Save changes
                await unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Token cleanup completed. Cleaned: {CleanedCount}, Removed: {RemovedCount}", 
                    cleanedCount, removedCount);
            }
            else
            {
                _logger.LogDebug("No expired tokens found during cleanup");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token cleanup operation");
        }
    }
}
