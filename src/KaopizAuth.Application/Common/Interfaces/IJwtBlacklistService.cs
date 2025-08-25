namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Service for managing blacklisted JWT tokens
/// </summary>
public interface IJwtBlacklistService
{
    /// <summary>
    /// Add a JWT token to the blacklist
    /// </summary>
    /// <param name="jwtToken">JWT token to blacklist</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task BlacklistTokenAsync(string jwtToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a JWT token is blacklisted
    /// </summary>
    /// <param name="jwtToken">JWT token to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is blacklisted</returns>
    Task<bool> IsTokenBlacklistedAsync(string jwtToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired tokens from blacklist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
