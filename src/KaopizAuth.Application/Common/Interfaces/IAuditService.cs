namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Interface for audit logging service
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log a successful login
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    Task LogSuccessfulLoginAsync(string userId, string email, string? ipAddress, string? userAgent = null);

    /// <summary>
    /// Log a failed login attempt
    /// </summary>
    /// <param name="email">Attempted email</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="reason">Failure reason</param>
    /// <param name="userAgent">User agent</param>
    Task LogFailedLoginAsync(string email, string? ipAddress, string reason, string? userAgent = null);

    /// <summary>
    /// Log a password change
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    Task LogPasswordChangeAsync(string userId, string email, string? ipAddress, string? userAgent = null);

    /// <summary>
    /// Log brute force attempt detection
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <param name="attemptCount">Number of failed attempts</param>
    /// <param name="email">Target email</param>
    Task LogBruteForceAttemptAsync(string? ipAddress, int attemptCount, string? email = null);

    /// <summary>
    /// Log sensitive data access
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="dataType">Type of sensitive data accessed</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    Task LogSensitiveDataAccessAsync(string userId, string dataType, string? ipAddress, string? userAgent = null);
}
