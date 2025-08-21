using KaopizAuth.Domain.Entities;

namespace KaopizAuth.Domain.Interfaces;

/// <summary>
/// Repository interface for RefreshToken operations
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>
    /// Gets a refresh token by token value
    /// </summary>
    /// <param name="token">Token value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RefreshToken or null if not found</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of refresh tokens</returns>
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active refresh tokens</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired refresh tokens
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of expired refresh tokens</returns>
    Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="ipAddress">IP address performing the revocation</param>
    /// <param name="reason">Reason for revocation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens revoked</returns>
    Task<int> RevokeAllUserTokensAsync(Guid userId, string ipAddress, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens (cleanup operation)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of tokens removed</returns>
    Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}
