using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Events;
using KaopizAuth.Domain.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace KaopizAuth.Infrastructure.Services.Domain;

/// <summary>
/// Domain service implementation for refresh token operations
/// </summary>
public class RefreshTokenDomainService : IRefreshTokenDomainService
{
    private readonly IConfiguration _configuration;
    private readonly IDeviceFingerprintService _deviceFingerprintService;

    public RefreshTokenDomainService(IConfiguration configuration, IDeviceFingerprintService deviceFingerprintService)
    {
        _configuration = configuration;
        _deviceFingerprintService = deviceFingerprintService;
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string ipAddress, bool rememberMe = false, string? userAgent = null)
    {
        var token = GenerateSecureToken();
        
        // Set expiration based on Remember Me flag
        var expirationDays = rememberMe 
            ? int.Parse(_configuration["JWT:PersistentTokenExpirationDays"] ?? "30") // 30 days for persistent sessions
            : int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");    // 7 days for regular sessions
            
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays);
        
        // Generate device fingerprint for security
        var deviceFingerprint = _deviceFingerprintService.GenerateFingerprint(ipAddress, userAgent);

        var refreshToken = RefreshToken.Create(
            token: token,
            expiresAt: expiresAt,
            userId: userId.ToString(),
            createdByIp: ipAddress,
            createdBy: "System",
            userAgent: userAgent,
            deviceFingerprint: deviceFingerprint,
            isPersistent: rememberMe
        );

        // TODO: Add domain event when inheritance issues are resolved
        // refreshToken.AddDomainEvent(new RefreshTokenCreatedEvent(userId, refreshToken.Id, ipAddress, expiresAt));

        return refreshToken;
    }

    public bool CanUseRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            return false;

        // Check if token is valid (not expired, not revoked, not soft deleted)
        return refreshToken.IsValid;
    }

    public void RevokeRefreshToken(RefreshToken refreshToken, string ipAddress, string? reason = null, string? replacedByToken = null)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        // Revoke the token
        refreshToken.Revoke(ipAddress, reason, replacedByToken, "System");

        // TODO: Add domain event when inheritance issues are resolved
    }

    public void RevokeAllUserTokens(IEnumerable<RefreshToken> refreshTokens, string ipAddress, string reason = "All tokens revoked")
    {
        if (refreshTokens == null)
            return;

        foreach (var token in refreshTokens.Where(t => t.IsValid))
        {
            RevokeRefreshToken(token, ipAddress, reason);
        }
    }

    public int CleanupExpiredTokens(IEnumerable<RefreshToken> refreshTokens, string ipAddress = "System")
    {
        if (refreshTokens == null)
            return 0;

        var expiredTokens = refreshTokens.Where(t => t.IsExpired && !t.IsRevoked).ToList();

        foreach (var token in expiredTokens)
        {
            RevokeRefreshToken(token, ipAddress, "Token expired - automatic cleanup");
        }

        return expiredTokens.Count;
    }

    public bool HasTooManyActiveTokens(IEnumerable<RefreshToken> activeTokens, int maxAllowedTokens = 5)
    {
        if (activeTokens == null)
            return false;

        var validTokenCount = activeTokens.Count(t => t.IsValid);
        return validTokenCount >= maxAllowedTokens;
    }

    public IEnumerable<RefreshToken> GetTokensToRevoke(IEnumerable<RefreshToken> activeTokens, int maxAllowedTokens = 5)
    {
        if (activeTokens == null)
            return Enumerable.Empty<RefreshToken>();

        var validTokens = activeTokens.Where(t => t.IsValid).ToList();

        if (validTokens.Count <= maxAllowedTokens)
            return Enumerable.Empty<RefreshToken>();

        // Return the oldest tokens that exceed the limit
        return validTokens
            .OrderBy(t => t.ExpiresAt) // Order by expiration date (oldest first)
            .Take(validTokens.Count - maxAllowedTokens);
    }

    private static string GenerateSecureToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
