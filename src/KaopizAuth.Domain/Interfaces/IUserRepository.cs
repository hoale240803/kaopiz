using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;

namespace KaopizAuth.Domain.Interfaces;

/// <summary>
/// Repository interface for ApplicationUser operations
/// </summary>
public interface IUserRepository : IRepository<ApplicationUser, Guid>
{
    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User or null if not found</returns>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User or null if not found</returns>
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by type
    /// </summary>
    /// <param name="userType">User type to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users</returns>
    Task<IEnumerable<ApplicationUser>> GetByUserTypeAsync(UserType userType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if email exists
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if username exists
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if username exists</returns>
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
}
