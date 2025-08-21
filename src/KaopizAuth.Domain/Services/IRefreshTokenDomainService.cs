using KaopizAuth.Domain.Entities;

namespace KaopizAuth.Domain.Services;

/// <summary>
/// Domain service interface for refresh token-related business operations
/// </summary>
public interface IRefreshTokenDomainService
{
    /// <summary>
    /// Generates a new refresh token for the specified user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ipAddress">The IP address where the token is being created</param>
    /// <returns>A new refresh token</returns>
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);

    /// <summary>
    /// Validates if a refresh token can be used (not expired, not revoked)
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate</param>
    /// <returns>True if token can be used, false otherwise</returns>
    bool CanUseRefreshToken(RefreshToken refreshToken);

    /// <summary>
    /// Revokes a refresh token and optionally marks it as replaced by another token
    /// </summary>
    /// <param name="refreshToken">The token to revoke</param>
    /// <param name="ipAddress">The IP address where revocation is happening</param>
    /// <param name="reason">Optional reason for revocation</param>
    /// <param name="replacedByToken">Optional replacement token</param>
    void RevokeRefreshToken(RefreshToken refreshToken, string ipAddress, string? reason = null, string? replacedByToken = null);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="refreshTokens">The user's refresh tokens</param>
    /// <param name="ipAddress">The IP address where revocation is happening</param>
    /// <param name="reason">Reason for revocation</param>
    void RevokeAllUserTokens(IEnumerable<RefreshToken> refreshTokens, string ipAddress, string reason = "All tokens revoked");

    /// <summary>
    /// Cleans up expired refresh tokens (marks them as revoked)
    /// </summary>
    /// <param name="refreshTokens">The tokens to check</param>
    /// <param name="ipAddress">The IP address for cleanup operation</param>
    /// <returns>Number of tokens cleaned up</returns>
    int CleanupExpiredTokens(IEnumerable<RefreshToken> refreshTokens, string ipAddress = "System");

    /// <summary>
    /// Checks if a user has too many active tokens (security measure)
    /// </summary>
    /// <param name="activeTokens">The user's active tokens</param>
    /// <param name="maxAllowedTokens">Maximum allowed active tokens (default: 5)</param>
    /// <returns>True if user has too many tokens, false otherwise</returns>
    bool HasTooManyActiveTokens(IEnumerable<RefreshToken> activeTokens, int maxAllowedTokens = 5);

    /// <summary>
    /// Gets the oldest active tokens that should be revoked when limit is exceeded
    /// </summary>
    /// <param name="activeTokens">The user's active tokens</param>
    /// <param name="maxAllowedTokens">Maximum allowed active tokens</param>
    /// <returns>Tokens that should be revoked</returns>
    IEnumerable<RefreshToken> GetTokensToRevoke(IEnumerable<RefreshToken> activeTokens, int maxAllowedTokens = 5);
}
