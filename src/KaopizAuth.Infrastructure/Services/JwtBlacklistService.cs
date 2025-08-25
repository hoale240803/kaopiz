using KaopizAuth.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace KaopizAuth.Infrastructure.Services;

/// <summary>
/// In-memory JWT token blacklist service
/// </summary>
public class JwtBlacklistService : IJwtBlacklistService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<JwtBlacklistService> _logger;
    private const string BLACKLIST_PREFIX = "jwt_blacklist_";

    public JwtBlacklistService(IMemoryCache cache, ILogger<JwtBlacklistService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task BlacklistTokenAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Parse the token to get expiration time
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.CanReadToken(jwtToken))
            {
                var token = tokenHandler.ReadJwtToken(jwtToken);
                var expiration = token.ValidTo;

                // Only cache if token hasn't expired yet
                if (expiration > DateTime.UtcNow)
                {
                    var cacheKey = BLACKLIST_PREFIX + GetTokenId(jwtToken);
                    _cache.Set(cacheKey, true, expiration);

                    _logger.LogInformation("JWT token blacklisted until {Expiration}", expiration);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting JWT token");
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            return Task.FromResult(false);
        }

        try
        {
            var cacheKey = BLACKLIST_PREFIX + GetTokenId(jwtToken);
            var isBlacklisted = _cache.TryGetValue(cacheKey, out _);
            
            return Task.FromResult(isBlacklisted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking JWT token blacklist");
            return Task.FromResult(false);
        }
    }

    public Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        // Memory cache automatically removes expired entries
        _logger.LogDebug("JWT token blacklist cleanup completed (automatic via memory cache)");
        return Task.CompletedTask;
    }

    private string GetTokenId(string jwtToken)
    {
        // Use SHA256 hash for consistent token identification
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(jwtToken));
        return Convert.ToBase64String(hashBytes);
    }
}
